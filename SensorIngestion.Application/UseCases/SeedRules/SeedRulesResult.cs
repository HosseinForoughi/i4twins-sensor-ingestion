namespace SensorIngestion.Application.UseCases.SeedRules;

public class SeedRulesResult
{
    public SeedRulesResult(int loadedCount, int insertedCount, int updatedCount)
    {
        LoadedCount = loadedCount;
        InsertedCount = insertedCount;
        UpdatedCount = updatedCount;
    }

    public int LoadedCount { get; }

    public int InsertedCount { get; }

    public int UpdatedCount { get; }
}