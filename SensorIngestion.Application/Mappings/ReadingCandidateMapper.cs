using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Validation.Data;
using SensorIngestion.Domain.ValueObjects;
using System.Globalization;

namespace SensorIngestion.Application.Mappings;

public static class ReadingCandidateMapper
{
    public static SensorReading ToEntity(ReadingCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return new SensorReading(deviceId: candidate.DeviceId!,
            metric: candidate.Metric!,
            timestamp: ParseTimestamp(candidate.Timestamp!),
            sequence: candidate.Sequence!.Value,
            value: candidate.Value!.Value);
    }

    public static ReadingNaturalKey ToNaturalKey(ReadingCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return new ReadingNaturalKey(candidate.DeviceId!,
            candidate.Metric!,
            ParseTimestamp(candidate.Timestamp!),
            candidate.Sequence!.Value);
    }

    private static DateTimeOffset ParseTimestamp(string timestamp)
    {
        if (!DateTimeOffset.TryParse(timestamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
        {
            throw new FormatException($"Timestamp '{timestamp}' is not a valid DateTimeOffset.");
        }

        return parsed.ToUniversalTime();
    }
}