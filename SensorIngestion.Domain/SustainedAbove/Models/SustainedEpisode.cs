namespace SensorIngestion.Domain.SustainedAbove.Models;

public class SustainedEpisode
{
    public SustainedEpisode(string ruleId,
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
        StartTs = startTs;
        EndTs = endTs;
        PeakValue = peakValue;
    }

    public string RuleId { get; }

    public string DeviceId { get; }

    public string Metric { get; }

    public DateTimeOffset StartTs { get; }

    public DateTimeOffset EndTs { get; }

    public double PeakValue { get; }

    public TimeSpan Duration => EndTs - StartTs;
}