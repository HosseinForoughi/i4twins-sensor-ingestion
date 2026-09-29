using SensorIngestion.Domain.Enums;

namespace SensorIngestion.Domain.Operators.Implementations;

public class EqualOperatorEvaluator : ThresholdComparisonOperatorEvaluator
{
    public override RuleOperator Operator => RuleOperator.Equal;

    protected override bool IsViolation(double value, double threshold) => value == threshold;

    protected override string BuildReason(string ruleId, double value, double threshold) =>
        $"Value {value} equals threshold {threshold} (rule '{ruleId}').";
}