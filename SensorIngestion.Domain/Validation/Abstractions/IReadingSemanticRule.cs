using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Domain.Validation.Abstractions;

public interface IReadingSemanticRule
{
    ReadingValidationError? Evaluate(ReadingCandidate candidate);
}