namespace SensorIngestion.Application.UseCases.EvaluateSustainedAbove;

/// <summary>
/// SustainedAbove evaluation and alerting counts for one pipeline run.
/// </summary>
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

    /// <summary>
    /// Enabled SustainedAbove rules evaluated in this run.
    /// </summary>
    public int RulesEvaluated { get; }

    /// <summary>
    /// Episodes whose event-time duration met the rule threshold.
    /// </summary>
    public int QualifyingEpisodes { get; }

    /// <summary>
    /// Episodes accepted after cooldown filtering (candidates for alert insert).
    /// </summary>
    public int AlertsAccepted { get; }

    /// <summary>
    /// Episodes suppressed because they fell inside an existing cooldown window.
    /// </summary>
    public int AlertsSuppressedByCooldown { get; }

    /// <summary>
    /// Alert rows newly inserted (0 when already present on an idempotent re-run).
    /// </summary>
    public int AlertsInserted { get; }

    /// <summary>
    /// Reading-level SustainedAbove violations attached during this run.
    /// </summary>
    public int ReadingViolationsAdded { get; }
}