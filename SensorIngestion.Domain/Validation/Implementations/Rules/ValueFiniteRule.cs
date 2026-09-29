using SensorIngestion.Domain.Validation.Abstractions;
using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Domain.Validation.Implementations.Rules;

public class ValueFiniteRule : IReadingSemanticRule
{
    public const string ErrorCode = "value_invalid";

    public ReadingValidationError? Evaluate(ReadingCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (!string.IsNullOrWhiteSpace(candidate.ValueRaw))
            return new ReadingValidationError(ErrorCode, $"value must be a finite number, got '{candidate.ValueRaw}'.");

        if (candidate.Value is null)
            return new ReadingValidationError(ErrorCode, "value is required.");

        if (!double.IsFinite(candidate.Value.Value))
            return new ReadingValidationError(ErrorCode, "value must be a finite number (not NaN or Infinity).");

        return null;
    }
}