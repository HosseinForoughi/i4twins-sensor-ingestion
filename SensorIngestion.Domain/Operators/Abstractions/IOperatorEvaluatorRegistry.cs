using SensorIngestion.Domain.Enums;

namespace SensorIngestion.Domain.Operators.Abstractions;

public interface IOperatorEvaluatorRegistry
{
    IOperatorEvaluator GetEvaluator(RuleOperator @operator);
}