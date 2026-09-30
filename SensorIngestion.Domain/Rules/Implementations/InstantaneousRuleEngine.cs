using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Operators.Abstractions;
using SensorIngestion.Domain.Rules.Abstractions;

namespace SensorIngestion.Domain.Rules.Implementations;

public class InstantaneousRuleEngine : IInstantaneousRuleEngine
{
    private readonly IOperatorEvaluatorRegistry _evaluatorRegistry;

    public InstantaneousRuleEngine(IOperatorEvaluatorRegistry evaluatorRegistry)
    {
        _evaluatorRegistry = evaluatorRegistry ?? throw new ArgumentNullException(nameof(evaluatorRegistry));
    }

    public void Evaluate(SensorReading reading, List<Rule> rules)
    {
        ArgumentNullException.ThrowIfNull(reading);
        ArgumentNullException.ThrowIfNull(rules);

        var applicableRules = rules.Where(rule => !rule.IsStateful && rule.AppliesTo(reading.DeviceId, reading.Metric)).ToList();

        if (applicableRules.Count == 0)
        {
            reading.MarkAsAcceptable();
            return;
        }

        var hasViolation = false;

        foreach (var rule in applicableRules)
        {
            var evaluator = _evaluatorRegistry.GetEvaluator(rule.Operator);

            if (!evaluator.CanEvaluate(rule))
            {
                throw new InvalidOperationException($"Evaluator for '{rule.Operator}' cannot evaluate rule '{rule.RuleId}'.");
            }

            var result = evaluator.Evaluate(rule, reading.Value);
            if (!result.IsViolated) continue;

            var violation = new ReadingViolation(rule.RuleId, result.Reason!);

            if (!hasViolation)
            {
                reading.MarkAsUnacceptable(violation);
                hasViolation = true;
            }
            else
            {
                reading.AddViolation(violation);
            }
        }

        if (!hasViolation) reading.MarkAsAcceptable();
    }
}
