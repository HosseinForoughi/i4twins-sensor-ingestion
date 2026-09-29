using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Domain.Validation.Abstractions;

public interface IReadingSemanticValidator
{
    ReadingValidationResult Validate(ReadingCandidate candidate);
}