namespace SensorIngestion.Application.Options;

public class ReadingsOptions
{
    public const string SectionName = "Readings";

    public string FilePath { get; set; } = "Data/readings.jsonl";
}