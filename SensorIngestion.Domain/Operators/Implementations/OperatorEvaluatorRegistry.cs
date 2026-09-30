using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.Operators.Abstractions;

namespace SensorIngestion.Domain.Operators.Implementations;

public class OperatorEvaluatorRegistry : IOperatorEvaluatorRegistry
{
    private readonly Dictionary<RuleOperator, IOperatorEvaluator> _evaluators;

    public OperatorEvaluatorRegistry(IEnumerable<IOperatorEvaluator> evaluators)
    {
        ArgumentNullException.ThrowIfNull(evaluators);

        _evaluators = evaluators.ToDictionary(e => e.Operator);

        if (_evaluators.Count == 0)
            throw new ArgumentException("At least one operator evaluator is required.", nameof(evaluators));
    }

    public IOperatorEvaluator GetEvaluator(RuleOperator @operator)
    {
        if (_evaluators.TryGetValue(@operator, out var evaluator))
            return evaluator;

        throw new InvalidOperationException($"No instantaneous operator evaluator is registered for '{@operator}'.");
    }
}