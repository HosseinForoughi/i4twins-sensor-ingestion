using SensorIngestion.Domain.Validation.Abstractions;
using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Domain.Validation.Implementations.Rules;

public class DeviceIdRequiredRule : IReadingSemanticRule
{
    public const string ErrorCode = "device_id_required";

    public ReadingValidationError? Evaluate(ReadingCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (string.IsNullOrWhiteSpace(candidate.DeviceId))
            return new ReadingValidationError(ErrorCode, "deviceId is required and must be non-empty.");

        return null;
    }
}