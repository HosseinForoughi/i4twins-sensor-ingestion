namespace SensorIngestion.Application.Options;

public class RulesOptions
{
    public const string SectionName = "Rules";

    public string SeedFilePath { get; set; } = "Data/rules.json";
}
