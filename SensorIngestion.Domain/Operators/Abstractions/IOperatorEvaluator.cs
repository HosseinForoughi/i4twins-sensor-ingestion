using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.Operators.Models;

namespace SensorIngestion.Domain.Operators.Abstractions;

public interface IOperatorEvaluator
{
    RuleOperator Operator { get; }

    bool CanEvaluate(Rule rule);

    OperatorEvaluationResult Evaluate(Rule rule, double value);
}