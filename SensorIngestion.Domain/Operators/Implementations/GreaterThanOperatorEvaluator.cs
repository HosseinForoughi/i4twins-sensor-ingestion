using SensorIngestion.Domain.Enums;

namespace SensorIngestion.Domain.Operators.Implementations;

public class GreaterThanOperatorEvaluator : ThresholdComparisonOperatorEvaluator
{
    public override RuleOperator Operator => RuleOperator.GreaterThan;

    protected override bool IsViolation(double value, double threshold) => value > threshold;

    protected override string BuildReason(string ruleId, double value, double threshold) =>
        $"Value {value} is greater than threshold {threshold} (rule '{ruleId}').";
}