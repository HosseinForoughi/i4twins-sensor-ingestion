# Sensor Ingestion

A .NET 8 service that takes messy sensor readings from a JSONL file, cleans them up, runs them through rules (including a stateful SustainedAbove check), stores alerts with cooldown, and exposes simple time-bucket aggregates over the acceptable data.

This is my submission for the danatadbir / i4Twins Backend Engineer technical task.

---

## What you need

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) only if you want to run integration / end-to-end tests (they spin up PostgreSQL with Testcontainers)

You do **not** need Docker just to run the API locally. The app uses SQLite out of the box.

---

## How to set up and run

From the repo root:

```bash
cd SensorIngestion
dotnet restore
dotnet run
```

Or open `SensorIngestion/SensorIngestion.sln` in Visual Studio / Rider / VS Code and run the API project.

By default the API listens on:

- http://localhost:5265
- https://localhost:7122

Swagger is at `/swagger` (for example http://localhost:5265/swagger). That is the easiest way to try the endpoints.

### What happens on startup

1. EF Core applies migrations and creates `sensor-ingestion.db` next to the app if needed
2. Rules from `Data/rules.json` are upserted into the database
3. The service waits for you to call ingest — it does not auto-process the readings file on boot

### Useful config (`appsettings.json`)

| Setting | What it does |
|---|---|
| `ConnectionStrings:SensorIngestion` | SQLite file path (`Data Source=sensor-ingestion.db`) |
| `Rules:SeedFilePath` | Where `rules.json` lives |
| `Readings:FilePath` | Where `readings.jsonl` lives |
| `Processing:BatchSize` | How many readings to handle per DB round-trip (default 500) |
| `Alerting:CooldownMinutes` | Alert cooldown window (default 5) |

### Try it

Process the sample file:

```http
POST /api/ingest
```

Then query aggregates for acceptable temperature on PUMP-01:

```http
GET /api/aggregates?deviceId=PUMP-01&metric=temperature&from=2025-06-01T08:00:00Z&to=2025-06-01T09:00:00Z&bucketSeconds=60
```

Calling ingest again on the same file is safe — you should not get duplicate readings or alerts.

---

## How to test

```bash
# Everything
dotnet test SensorIngestion.Tests/SensorIngestion.Tests.csproj

# Fast path (no Docker)
dotnet test SensorIngestion.Tests/SensorIngestion.Tests.csproj --filter "FullyQualifiedName~Unit|FullyQualifiedName~Smoke"

# Needs Docker
dotnet test SensorIngestion.Tests/SensorIngestion.Tests.csproj --filter "FullyQualifiedName~Integration|FullyQualifiedName~EndToEnd"
```

Tests are split by concern:

- **Unit** — domain rules, operators, SustainedAbove, cooldown, ingest use case (Moq)
- **Integration** — repositories / aggregates against PostgreSQL in Docker
- **End-to-end** — real HTTP against the API host + Docker DB
- **Smoke** — app boots and DI resolves (catches wiring mistakes early)

Naming follows `Method_Scenario_Expected`, and each test is arranged as Arrange / Act / Assert.

---

## Why SQLite

I chose **SQLite** because it is a real relational database, the file can sit next to the project, and you can run the whole service without bringing up another container. That keeps local setup simple for a short take-home.

Storage is behind repository / query interfaces, so swapping later is straightforward. A time-series database (or Postgres/Timescale) would be a natural next step for high-volume sensor history; the domain and use cases would stay the same — mainly the Infrastructure package would change.

Integration and E2E tests still use PostgreSQL in Docker so persistence is exercised on a server engine, not only on SQLite.

---

## Architecture in short

Clean Architecture, four projects:

- **Domain** — entities, validation, dedupe, operators, SustainedAbove, alerting
- **Application** — use cases, ports, options
- **Infrastructure** — EF Core, SQLite, JSON/JSONL loaders, repositories
- **Api** — controllers, Swagger, logging / error handling, seed data files

Controllers stay thin. Business rules do not know about EF or HTTP.

### Rules

Rules come from `Data/rules.json` and are upserted by `ruleId` at startup. Turning a rule on/off or changing a threshold is a data change, not a code change.

Operators are pluggable (`IOperatorEvaluator` + registry). Adding a new instantaneous operator means a new class and a DI registration — the evaluation loop does not need to be rewritten.

- A rule without `deviceId` applies to every device for that metric
- Disabled rules are skipped
- If no enabled rule applies, the reading is treated as **acceptable**
- Invalid or duplicate readings never reach rule evaluation, so they are not counted as “unacceptable”

### Deduplication — first wins

Two readings are duplicates when `(deviceId, metric, ts, seq)` matches. I keep the **first** occurrence in file order and drop the rest. That felt predictable for a fixed input file and is easy to explain and test.

### SustainedAbove — event time, per stream

The file is not sorted, so SustainedAbove sorts by event time `(timestamp, sequence)` before scanning.

- An episode starts when the value goes above the threshold
- It ends when the value drops back to ≤ threshold (or at the last above-threshold point if still open at the end of data)
- It only qualifies if `end − start ≥ durationSeconds`
- Evaluation is done **per `(deviceId, metric)` stream**, in batches from the database, so we do not pull the whole table into memory at once

Batch + sort is deliberate: for a finite file it is simple and correct. A live stream would want incremental state instead; that trade-off is acceptable here.

### Alerts and cooldown

SustainedAbove produces **alerts** (rule, device, metric, start, end, peak), not only per-reading flags. After an alert for `(ruleId, deviceId, metric)`, no new alert is accepted until **5 minutes** after that alert’s `startTs` (`Alerting:CooldownMinutes`). Episodes inside the cooldown window are suppressed. Re-running the same file does not insert the same alert twice.

### Aggregation

`GET /api/aggregates` only uses readings classified **Acceptable**. Buckets are half-open `[from, to)` with width `bucketSeconds`. Empty buckets are left out of the response.

### Processing in batches

Ingest streams the JSONL file in chunks, validates, dedupes across chunks with a compact key set, and inserts per batch. Instantaneous rules page unprocessed rows from the DB. SustainedAbove loads one device/metric stream at a time. Batch size is configurable via `Processing:BatchSize`.

### Logging and errors

Every request gets an `X-Correlation-Id` (yours or generated) plus a trace/activity span. Requests and pipeline stages are logged. Unexpected failures return a JSON error body with `error`, `correlationId`, and `traceId` (extra `detail` in Development for 5xx).

### Idempotency

| What | Key |
|---|---|
| Reading | `(deviceId, metric, timestamp, sequence)` |
| Alert | `(ruleId, deviceId, metric, startTs)` |
| Rule seed | `ruleId` (upsert) |

---

## Project layout

```
SensorIngestion.Domain/
SensorIngestion.Application/
SensorIngestion.Infrastructure/
SensorIngestion/                 # API, appsettings, Data/rules.json, Data/readings.jsonl
SensorIngestion.Tests/
docs/                            # original task PDF
```

---

## AI tool disclosure

I used **Cursor** while building this project — mainly for naming, tests, cleaning up structure, and logging/observability helpers. The design choices (SQLite, first-wins dedupe, SustainedAbove event-time policy, cooldown, batching, Clean Architecture split) are mine; I reviewed and own the code that shipped.
