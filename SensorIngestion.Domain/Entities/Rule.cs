using SensorIngestion.Domain.Enums;

namespace SensorIngestion.Domain.Entities;

public class Rule
{
    public long Id { get; private set; }

    public string RuleId { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public bool Enabled { get; private set; }

    public string Metric { get; private set; } = null!;

    public string? DeviceId { get; private set; }

    public RuleOperator Operator { get; private set; }

    public double? Threshold { get; private set; }

    public double? MinValue { get; private set; }

    public double? MaxValue { get; private set; }

    public int? DurationSeconds { get; private set; }

    public DateTimeOffset CreationDateTime { get; private set; }

    public DateTimeOffset DbEntryDateTime { get; private set; }

    /// <summary>
    /// EF Core
    /// </summary>
    private Rule()
    {
    }

    public Rule(string ruleId,
        string name,
        bool enabled,
        string metric,
        RuleOperator @operator,
        string? deviceId = null,
        double? threshold = null,
        double? minValue = null,
        double? maxValue = null,
        int? durationSeconds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        if (deviceId is not null && string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("DeviceId, when provided, must be non-empty.", nameof(deviceId));

        ValidateOperatorParameters(@operator, threshold, minValue, maxValue, durationSeconds);

        RuleId = ruleId;
        Name = name;
        Enabled = enabled;
        Metric = metric;
        DeviceId = deviceId;
        Operator = @operator;
        Threshold = threshold;
        MinValue = minValue;
        MaxValue = maxValue;
        DurationSeconds = durationSeconds;
        CreationDateTime = DateTimeOffset.UtcNow;
    }

    public void SyncFromSeed(string name,
        bool enabled,
        string metric,
        RuleOperator @operator,
        string? deviceId = null,
        double? threshold = null,
        double? minValue = null,
        double? maxValue = null,
        int? durationSeconds = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        if (deviceId is not null && string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("DeviceId, when provided, must be non-empty.", nameof(deviceId));

        ValidateOperatorParameters(@operator, threshold, minValue, maxValue, durationSeconds);

        Name = name;
        Enabled = enabled;
        Metric = metric;
        DeviceId = deviceId;
        Operator = @operator;
        Threshold = threshold;
        MinValue = minValue;
        MaxValue = maxValue;
        DurationSeconds = durationSeconds;
    }

    public bool AppliesTo(string deviceId, string metric)
    {
        if (!Enabled)
            return false;

        if (!string.Equals(Metric, metric, StringComparison.Ordinal))
            return false;

        if (DeviceId is null)
            return true;

        return string.Equals(DeviceId, deviceId, StringComparison.Ordinal);
    }

    public bool IsStateful => Operator == RuleOperator.SustainedAbove;

    private static void ValidateOperatorParameters(RuleOperator @operator,
        double? threshold,
        double? minValue,
        double? maxValue,
        int? durationSeconds)
    {
        switch (@operator)
        {
            case RuleOperator.GreaterThan:
            case RuleOperator.GreaterThanOrEqual:
            case RuleOperator.LessThan:
            case RuleOperator.LessThanOrEqual:
            case RuleOperator.Equal:
                RequireFinite(threshold, nameof(threshold), @operator);
                break;

            case RuleOperator.Between:
                RequireFinite(minValue, nameof(minValue), @operator);
                RequireFinite(maxValue, nameof(maxValue), @operator);
                if (minValue > maxValue)
                    throw new ArgumentException("MinValue must be ≤ MaxValue for Between.", nameof(minValue));
                break;

            case RuleOperator.SustainedAbove:
                RequireFinite(threshold, nameof(threshold), @operator);
                if (durationSeconds is null or <= 0)
                    throw new ArgumentException("DurationSeconds must be a positive integer for SustainedAbove.", nameof(durationSeconds));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(@operator), @operator, "Unsupported operator.");
        }
    }

    private static void RequireFinite(double? value, string paramName, RuleOperator @operator)
    {
        if (value is null || !double.IsFinite(value.Value))
            throw new ArgumentException($"{paramName} must be a finite number for {@operator}.", paramName);
    }
}
