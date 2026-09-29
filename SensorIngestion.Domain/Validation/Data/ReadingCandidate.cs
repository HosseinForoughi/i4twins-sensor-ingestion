namespace SensorIngestion.Domain.Validation.Data;

public class ReadingCandidate
{
    public string? DeviceId { get; init; }

    public string? Metric { get; init; }

    public string? Timestamp { get; init; }

    public double? Value { get; init; }

    public string? ValueRaw { get; init; }

    public long? Sequence { get; init; }
}