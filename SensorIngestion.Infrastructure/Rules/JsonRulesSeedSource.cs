using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Options;
using SensorIngestion.Domain.Entities;
using System.Text.Json;

namespace SensorIngestion.Infrastructure.Rules;

public class JsonRulesSeedSource : IRulesSeedSource
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private readonly RulesOptions _options;
    private readonly ILogger<JsonRulesSeedSource> _logger;

    public JsonRulesSeedSource(IOptions<RulesOptions> options, ILogger<JsonRulesSeedSource> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Rule>> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(_options.SeedFilePath);

        if (!File.Exists(path)) throw new FileNotFoundException($"Rules seed file was not found at '{path}'.", path);

        _logger.LogInformation("Reading rules seed file from {Path}.", path);

        await using var stream = File.OpenRead(path);
        var dtos = await JsonSerializer.DeserializeAsync<List<RuleSeedDto>>(stream, SerializerOptions, cancellationToken);

        if (dtos is null || dtos.Count == 0) throw new InvalidOperationException($"Rules seed file '{path}' is empty or invalid.");

        var rules = new List<Rule>();
        dtos.ForEach(dto => rules.Add(MapToDomain(dto)));

        return rules;
    }

    private static string ResolvePath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath)) throw new InvalidOperationException("Rules:SeedFilePath is not configured.");

        if (Path.IsPathRooted(configuredPath)) return configuredPath;

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configuredPath));
    }

    private static Rule MapToDomain(RuleSeedDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RuleId))
            throw new InvalidOperationException("A seed rule is missing ruleId.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException($"Rule '{dto.RuleId}' is missing name.");

        if (string.IsNullOrWhiteSpace(dto.Metric))
            throw new InvalidOperationException($"Rule '{dto.RuleId}' is missing metric.");

        return new Rule(ruleId: dto.RuleId,
            name: dto.Name,
            enabled: dto.Enabled,
            metric: dto.Metric,
            @operator: dto.ParseOperator(),
            deviceId: string.IsNullOrWhiteSpace(dto.DeviceId) ? null : dto.DeviceId,
            threshold: dto.Threshold,
            minValue: dto.MinValue,
            maxValue: dto.MaxValue,
            durationSeconds: dto.DurationSeconds);
    }
}