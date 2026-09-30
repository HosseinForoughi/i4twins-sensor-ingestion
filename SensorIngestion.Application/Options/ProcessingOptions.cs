namespace SensorIngestion.Application.Options;

public class ProcessingOptions
{
    public const string SectionName = "Processing";

    public int BatchSize { get; set; } = 500;
}