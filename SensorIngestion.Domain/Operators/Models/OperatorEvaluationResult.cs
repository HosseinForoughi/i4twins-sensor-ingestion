namespace SensorIngestion.Domain.Operators.Models;

public class OperatorEvaluationResult
{
    private OperatorEvaluationResult(bool isViolated, string? reason)
    {
        IsViolated = isViolated;
        Reason = reason;
    }

    public bool IsViolated { get; }

    public string? Reason { get; }

    public static OperatorEvaluationResult Satisfied()
    {
        return new(isViolated: false, reason: null);
    }

    public static OperatorEvaluationResult Violated(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return new(isViolated: true, reason: reason);
    }
}
