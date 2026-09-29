using SensorIngestion.Domain.Enums;

namespace SensorIngestion.Domain.Operators.Implementations;

public class LessThanOperatorEvaluator : ThresholdComparisonOperatorEvaluator
{
    public override RuleOperator Operator => RuleOperator.LessThan;

    protected override bool IsViolation(double value, double threshold) => value < threshold;

    protected override string BuildReason(string ruleId, double value, double threshold) =>
        $"Value {value} is less than threshold {threshold} (rule '{ruleId}').";
}