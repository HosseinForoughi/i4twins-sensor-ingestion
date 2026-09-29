using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.Operators.Abstractions;
using SensorIngestion.Domain.Operators.Models;

namespace SensorIngestion.Domain.Operators.Implementations;

public class BetweenOperatorEvaluator : IOperatorEvaluator
{
    public RuleOperator Operator => RuleOperator.Between;

    public bool CanEvaluate(Rule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.Operator == RuleOperator.Between
               && rule.MinValue is not null
               && rule.MaxValue is not null
               && double.IsFinite(rule.MinValue.Value)
               && double.IsFinite(rule.MaxValue.Value)
               && rule.MinValue.Value <= rule.MaxValue.Value;
    }

    public OperatorEvaluationResult Evaluate(Rule rule, double value)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (!CanEvaluate(rule))
        {
            throw new InvalidOperationException($"Rule '{rule.RuleId}' is not a valid Between rule with a finite [MinValue, MaxValue] range.");
        }

        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a finite number.");

        var min = rule.MinValue!.Value;
        var max = rule.MaxValue!.Value;

        if (value >= min && value <= max)
        {
            return OperatorEvaluationResult.Violated(
                $"Value {value} is between {min} and {max} inclusive (rule '{rule.RuleId}').");
        }

        return OperatorEvaluationResult.Satisfied();
    }
}