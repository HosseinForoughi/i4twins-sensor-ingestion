using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.Operators.Abstractions;
using SensorIngestion.Domain.Operators.Models;

namespace SensorIngestion.Domain.Operators.Implementations;

public abstract class ThresholdComparisonOperatorEvaluator : IOperatorEvaluator
{
    public abstract RuleOperator Operator { get; }

    public bool CanEvaluate(Rule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.Operator == Operator
               && rule.Threshold is not null
               && double.IsFinite(rule.Threshold.Value);
    }

    public OperatorEvaluationResult Evaluate(Rule rule, double value)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (!CanEvaluate(rule))
        {
            throw new InvalidOperationException(
                $"Rule '{rule.RuleId}' is not a valid {Operator} rule with a finite threshold.");
        }

        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a finite number.");

        var threshold = rule.Threshold!.Value;

        if (IsViolation(value, threshold))
            return OperatorEvaluationResult.Violated(BuildReason(rule.RuleId, value, threshold));

        return OperatorEvaluationResult.Satisfied();
    }

    protected abstract bool IsViolation(double value, double threshold);

    protected abstract string BuildReason(string ruleId, double value, double threshold);
}
