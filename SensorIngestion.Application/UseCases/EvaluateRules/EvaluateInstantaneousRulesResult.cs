namespace SensorIngestion.Application.UseCases.EvaluateRules;

/// <summary>
/// Instantaneous rule-evaluation counts for one pipeline run.
/// </summary>
public class EvaluateInstantaneousRulesResult
{
    public EvaluateInstantaneousRulesResult(int rulesLoaded,
        int readingsEvaluated,
        int acceptableReadings,
        int unacceptableReadings,
        int ruleViolations)
    {
        RulesLoaded = rulesLoaded;
        ReadingsEvaluated = readingsEvaluated;
        AcceptableReadings = acceptableReadings;
        UnacceptableReadings = unacceptableReadings;
        RuleViolations = ruleViolations;
    }

    /// <summary>
    /// Enabled non-stateful rules used for classification.
    /// </summary>
    public int RulesLoaded { get; }

    /// <summary>
    /// Readings that were still unprocessed and were classified in this run.
    /// </summary>
    public int ReadingsEvaluated { get; }

    /// <summary>
    /// Readings that satisfied all applicable instantaneous rules.
    /// </summary>
    public int AcceptableReadings { get; }

    /// <summary>
    /// Readings that violated at least one applicable instantaneous rule.
    /// </summary>
    public int UnacceptableReadings { get; }

    /// <summary>
    /// Total instantaneous rule violation records attached to readings.
    /// </summary>
    public int RuleViolations { get; }
}