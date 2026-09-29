using SensorIngestion.Domain.Validation.Abstractions;
using SensorIngestion.Domain.Validation.Data;
using System.Globalization;

namespace SensorIngestion.Domain.Validation.Implementations.Rules;

public class TimestampValidRule : IReadingSemanticRule
{
    public const string ErrorCode = "timestamp_invalid";

    public ReadingValidationError? Evaluate(ReadingCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (string.IsNullOrWhiteSpace(candidate.Timestamp))
            return new ReadingValidationError(ErrorCode, "ts is required.");

        var text = candidate.Timestamp.Trim();

        if (!DateTimeOffset.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out _))
        {
            return new ReadingValidationError(ErrorCode, $"ts is not a valid timestamp: '{candidate.Timestamp}'.");
        }

        return null;
    }
}