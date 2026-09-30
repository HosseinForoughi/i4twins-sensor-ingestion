namespace SensorIngestion.Application.Options;

public class AlertingOptions
{
    public const string SectionName = "Alerting";

    public int CooldownMinutes { get; set; } = 5;
}