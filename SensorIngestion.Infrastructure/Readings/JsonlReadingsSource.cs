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

    public async Task<ReadingsLoadResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(_options.FilePath);

        if (!File.Exists(path)) throw new FileNotFoundException($"Readings file was not found at '{path}'.", path);

        _logger.LogInformation("Reading JSONL file from {Path}.", path);

        var candidates = new List<ReadingCandidate>();
        var totalLinesRead = 0;
        var malformedLineCount = 0;

        using var reader = new StreamReader(path);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;

            totalLinesRead++;

            if (string.IsNullOrWhiteSpace(line))
            {
                malformedLineCount++;
                _logger.LogWarning("Malformed readings line {LineNumber}: empty line.", totalLinesRead);
                continue;
            }

            if (!TryParseCandidate(line, totalLinesRead, out var candidate))
            {
                malformedLineCount++;
                continue;
            }

            candidates.Add(candidate!);
        }

        _logger.LogInformation("JSONL load finished. TotalLines={Total}, Parsed={Parsed}, Malformed={Malformed}.",
            totalLinesRead,
            candidates.Count,
            malformedLineCount);

        return new ReadingsLoadResult(totalLinesRead, candidates, malformedLineCount);
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

    private static string ResolvePath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            throw new InvalidOperationException("Readings:FilePath is not configured.");

        if (Path.IsPathRooted(configuredPath))
            return configuredPath;

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));
    }
}