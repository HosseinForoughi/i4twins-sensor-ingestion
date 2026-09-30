using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Options;
using SensorIngestion.Application.Readings;
using SensorIngestion.Domain.Validation.Data;
using System.Text.Json;

namespace SensorIngestion.Infrastructure.Readings;

public class JsonlReadingsSource : IReadingsSource
{
    private readonly ReadingsOptions _options;
    private readonly ILogger<JsonlReadingsSource> _logger;

    public JsonlReadingsSource(IOptions<ReadingsOptions> options, ILogger<JsonlReadingsSource> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IReadingsBatchReader> OpenBatchesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be positive.");

        cancellationToken.ThrowIfCancellationRequested();

        var path = ResolvePath(_options.FilePath);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Readings file was not found at '{path}'.", path);

        _logger.LogInformation("Reading JSONL file from {Path} in batches of {BatchSize}.", path, batchSize);

        IReadingsBatchReader reader = new JsonlReadingsBatchReader(path, batchSize, _logger);
        return Task.FromResult(reader);
    }

    private static string ResolvePath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            throw new InvalidOperationException("Readings:FilePath is not configured.");

        if (Path.IsPathRooted(configuredPath))
            return configuredPath;

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));
    }

    private class JsonlReadingsBatchReader : IReadingsBatchReader
    {
        private readonly StreamReader _reader;
        private readonly int _batchSize;
        private readonly ILogger _logger;
        private int _totalLines;
        private int _totalParsed;
        private int _totalMalformed;
        private bool _completed;
        private bool _disposed;

        public JsonlReadingsBatchReader(string path, int batchSize, ILogger logger)
        {
            _reader = new StreamReader(path);
            _batchSize = batchSize;
            _logger = logger;
        }

        public async Task<ReadingsBatch?> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_completed)
                return null;

            var candidates = new List<ReadingCandidate>(_batchSize);
            var linesRead = 0;
            var malformedLineCount = 0;

            while (!_reader.EndOfStream && candidates.Count < _batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await _reader.ReadLineAsync(cancellationToken);
                if (line is null)
                    break;

                linesRead++;
                _totalLines++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    malformedLineCount++;
                    _totalMalformed++;
                    _logger.LogWarning("Malformed readings line {LineNumber}: empty line.", _totalLines);
                    continue;
                }

                if (!TryParseCandidate(line, _totalLines, out var candidate))
                {
                    malformedLineCount++;
                    _totalMalformed++;
                    continue;
                }

                candidates.Add(candidate!);
                _totalParsed++;
            }

            if (linesRead == 0 && candidates.Count == 0)
            {
                Complete();
                return null;
            }

            if (_reader.EndOfStream)
                Complete();

            return new ReadingsBatch(candidates, linesRead, malformedLineCount);
        }

        public ValueTask DisposeAsync()
        {
            if (_disposed)
                return ValueTask.CompletedTask;

            _disposed = true;

            if (!_completed)
                Complete();

            _reader.Dispose();
            return ValueTask.CompletedTask;
        }

        private void Complete()
        {
            if (_completed)
                return;

            _completed = true;
            _logger.LogInformation("JSONL load finished. TotalLines={Total}, Parsed={Parsed}, Malformed={Malformed}.", _totalLines,
                _totalParsed,
                _totalMalformed);
        }

        private bool TryParseCandidate(string line, int lineNumber, out ReadingCandidate? candidate)
        {
            candidate = null;

            try
            {
                using var document = JsonDocument.Parse(line);
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    _logger.LogWarning("Malformed readings line {LineNumber}: root JSON value is not an object.", lineNumber);
                    return false;
                }

                candidate = new ReadingCandidate
                {
                    DeviceId = ReadString(root, "deviceId"),
                    Metric = ReadString(root, "metric"),
                    Timestamp = ReadString(root, "ts"),
                    Sequence = ReadInt64(root, "seq"),
                    Value = ReadNumber(root, "value", out var valueRaw),
                    ValueRaw = valueRaw
                };

                return true;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Malformed readings line {LineNumber}: invalid JSON.", lineNumber);
                return false;
            }
        }

        private static string? ReadString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element))
                return null;

            return element.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => element.GetString(),
                _ => element.ToString()
            };
        }

        private static long? ReadInt64(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var element))
                return null;

            if (element.ValueKind == JsonValueKind.Null)
                return null;

            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var value))
                return value;

            return null;
        }

        private static double? ReadNumber(JsonElement root, string propertyName, out string? valueRaw)
        {
            valueRaw = null;

            if (!root.TryGetProperty(propertyName, out var element))
                return null;

            if (element.ValueKind == JsonValueKind.Null)
                return null;

            if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var number))
                return number;

            if (element.ValueKind == JsonValueKind.String)
            {
                valueRaw = element.GetString();
                return null;
            }

            valueRaw = element.ToString();
            return null;
        }
    }
}