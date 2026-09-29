using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.ValueObjects;

namespace SensorIngestion.Domain.Entities;

public class SensorReading
{
    public long Id { get; private set; }

    public string DeviceId { get; private set; } = null!;

    public string Metric { get; private set; } = null!;

    public DateTimeOffset Timestamp { get; private set; }

    public long Sequence { get; private set; }

    public double Value { get; private set; }

    public ReadingClassification Classification { get; private set; } = ReadingClassification.Unprocessed;

    public DateTimeOffset CreationDateTime { get; private set; }

    public DateTimeOffset DbEntryDateTime { get; private set; }

    private readonly List<ReadingViolation> _violations = [];

    public IReadOnlyCollection<ReadingViolation> Violations => _violations.AsReadOnly();

    public ReadingNaturalKey NaturalKey => new(DeviceId, Metric, Timestamp, Sequence);

    /// <summary>
    /// EF Core
    /// </summary>
    private SensorReading()
    {
    }

    public SensorReading(string deviceId,
        string metric,
        DateTimeOffset timestamp,
        long sequence,
        double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a finite number.");

        if (timestamp.Offset != TimeSpan.Zero)
            timestamp = timestamp.ToUniversalTime();

        DeviceId = deviceId;
        Metric = metric;
        Timestamp = timestamp;
        Sequence = sequence;
        Value = value;
        CreationDateTime = DateTimeOffset.UtcNow;
    }

    public void MarkAsAcceptable()
    {
        if (_violations.Count > 0)
            throw new InvalidOperationException("Cannot mark a reading with violations as acceptable.");

        Classification = ReadingClassification.Acceptable;
    }

    public void MarkAsUnacceptable(ReadingViolation violation)
    {
        ArgumentNullException.ThrowIfNull(violation);

        if (_violations.Any(v => v.RuleId == violation.RuleId))
            throw new InvalidOperationException($"Violation for rule '{violation.RuleId}' already exists on this reading.");

        _violations.Add(violation);
        Classification = ReadingClassification.Unacceptable;
    }

    public void AddViolation(ReadingViolation violation)
    {
        ArgumentNullException.ThrowIfNull(violation);

        if (Classification == ReadingClassification.Acceptable)
            throw new InvalidOperationException("Cannot add a violation to an acceptable reading.");

        if (_violations.Any(v => v.RuleId == violation.RuleId))
            throw new InvalidOperationException($"Violation for rule '{violation.RuleId}' already exists on this reading.");

        _violations.Add(violation);
        Classification = ReadingClassification.Unacceptable;
    }
}
