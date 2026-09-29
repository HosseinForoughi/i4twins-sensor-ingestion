using SensorIngestion.Domain.Validation.Abstractions;
using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Domain.Validation.Implementations.Rules;

public class SequenceRequiredRule : IReadingSemanticRule
{
    public const string ErrorCode = "sequence_required";

    public ReadingValidationError? Evaluate(ReadingCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (candidate.Sequence is null)
            return new ReadingValidationError(ErrorCode, "seq is required.");

        return null;
    }
}