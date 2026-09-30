using SensorIngestion.Domain.Enums;
using System.Text.Json.Serialization;

namespace SensorIngestion.Infrastructure.Rules;

internal class RuleSeedDto
{
    [JsonPropertyName("ruleId")]
    public string? RuleId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("metric")]
    public string? Metric { get; set; }

    [JsonPropertyName("deviceId")]
    public string? DeviceId { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("threshold")]
    public double? Threshold { get; set; }

    [JsonPropertyName("minValue")]
    public double? MinValue { get; set; }

    [JsonPropertyName("maxValue")]
    public double? MaxValue { get; set; }

    [JsonPropertyName("durationSeconds")]
    public int? DurationSeconds { get; set; }

    public RuleOperator ParseOperator()
    {
        if (string.IsNullOrWhiteSpace(Operator))
            throw new InvalidOperationException($"Rule '{RuleId}' is missing operator.");

        if (!Enum.TryParse<RuleOperator>(Operator, ignoreCase: true, out var parsed))
            throw new InvalidOperationException($"Rule '{RuleId}' has unknown operator '{Operator}'.");

        return parsed;
    }
}