namespace SensorIngestion.Application.UseCases.EvaluateRules;

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

    public int RulesLoaded { get; }

    public int ReadingsEvaluated { get; }

    public int AcceptableReadings { get; }

    public int UnacceptableReadings { get; }

    public int RuleViolations { get; }
}