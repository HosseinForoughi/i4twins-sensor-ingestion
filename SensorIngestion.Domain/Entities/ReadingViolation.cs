namespace SensorIngestion.Domain.Entities;

public class ReadingViolation
{
    public long Id { get; private set; }

    public long ReadingId { get; private set; }

    public string RuleId { get; private set; } = null!;

    public string Reason { get; private set; } = null!;

    public DateTimeOffset CreationDateTime { get; private set; }

    public DateTimeOffset DbEntryDateTime { get; private set; }

    public SensorReading? Reading { get; private set; }

    /// <summary>
    /// EF Core
    /// </summary>
    private ReadingViolation()
    {
    }

    public ReadingViolation(string ruleId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        RuleId = ruleId;
        Reason = reason;
        CreationDateTime = DateTimeOffset.UtcNow;
    }
}
