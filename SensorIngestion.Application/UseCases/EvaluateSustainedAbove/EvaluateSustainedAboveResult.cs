namespace SensorIngestion.Application.UseCases.EvaluateSustainedAbove;

public class EvaluateSustainedAboveResult
{
    public EvaluateSustainedAboveResult(int rulesEvaluated,
        int qualifyingEpisodes,
        int alertsAccepted,
        int alertsSuppressedByCooldown,
        int alertsInserted,
        int readingViolationsAdded)
    {
        RulesEvaluated = rulesEvaluated;
        QualifyingEpisodes = qualifyingEpisodes;
        AlertsAccepted = alertsAccepted;
        AlertsSuppressedByCooldown = alertsSuppressedByCooldown;
        AlertsInserted = alertsInserted;
        ReadingViolationsAdded = readingViolationsAdded;
    }

    public int RulesEvaluated { get; }

    public int QualifyingEpisodes { get; }

    public int AlertsAccepted { get; }

    public int AlertsSuppressedByCooldown { get; }

    public int AlertsInserted { get; }

    public int ReadingViolationsAdded { get; }
}