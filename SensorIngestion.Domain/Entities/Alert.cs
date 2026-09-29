using SensorIngestion.Domain.ValueObjects;

namespace SensorIngestion.Domain.Entities;

public class Alert
{
    public long Id { get; private set; }

    public string RuleId { get; private set; } = null!;

    public string DeviceId { get; private set; } = null!;

    public string Metric { get; private set; } = null!;

    public DateTimeOffset StartTs { get; private set; }

    public DateTimeOffset EndTs { get; private set; }

    public double PeakValue { get; private set; }

    public DateTimeOffset CreationDateTime { get; private set; }

    public DateTimeOffset DbEntryDateTime { get; private set; }

    public AlertNaturalKey NaturalKey => new(RuleId, DeviceId, Metric, StartTs);

    /// <summary>
    /// EF Core
    /// </summary>
    private Alert()
    {
    }

    public Alert(string ruleId,
        string deviceId,
        string metric,
        DateTimeOffset startTs,
        DateTimeOffset endTs,
        double peakValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        if (endTs < startTs)
            throw new ArgumentException("EndTs must be greater than or equal to StartTs.", nameof(endTs));

        if (!double.IsFinite(peakValue))
            throw new ArgumentOutOfRangeException(nameof(peakValue), "PeakValue must be a finite number.");

        RuleId = ruleId;
        DeviceId = deviceId;
        Metric = metric;
        StartTs = startTs.ToUniversalTime();
        EndTs = endTs.ToUniversalTime();
        PeakValue = peakValue;
        CreationDateTime = DateTimeOffset.UtcNow;
    }

    public bool IsInCooldownWindow(DateTimeOffset candidateStartTs, TimeSpan cooldown)
    {
        if (cooldown <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(cooldown), "Cooldown must be positive.");

        return (candidateStartTs >= StartTs) && (candidateStartTs < StartTs.Add(cooldown));
    }
}
