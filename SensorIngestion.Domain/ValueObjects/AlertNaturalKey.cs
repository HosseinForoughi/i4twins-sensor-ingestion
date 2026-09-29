namespace SensorIngestion.Domain.ValueObjects;

public readonly record struct AlertNaturalKey(string RuleId,
    string DeviceId,
    string Metric,
    DateTimeOffset StartTs)
{
    public override string ToString() =>
        $"{RuleId}|{DeviceId}|{Metric}|{StartTs:O}";
}