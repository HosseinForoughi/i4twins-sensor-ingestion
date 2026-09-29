namespace SensorIngestion.Domain.ValueObjects;

public readonly record struct ReadingNaturalKey(string DeviceId,
    string Metric,
    DateTimeOffset Timestamp,
    long Sequence)
{
    public override string ToString() =>
        $"{DeviceId}|{Metric}|{Timestamp:O}|{Sequence}";
}