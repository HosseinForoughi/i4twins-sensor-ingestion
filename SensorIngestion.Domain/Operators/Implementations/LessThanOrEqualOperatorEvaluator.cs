using SensorIngestion.Domain.Enums;

namespace SensorIngestion.Domain.Operators.Implementations;

public class LessThanOrEqualOperatorEvaluator : ThresholdComparisonOperatorEvaluator
{
    public override RuleOperator Operator => RuleOperator.LessThanOrEqual;

    protected override bool IsViolation(double value, double threshold) => value <= threshold;

    protected override string BuildReason(string ruleId, double value, double threshold) =>
        $"Value {value} is less than or equal to threshold {threshold} (rule '{ruleId}').";
}