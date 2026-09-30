using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Application.Readings;

public class ReadingsLoadResult
{
    public ReadingsLoadResult(int totalLinesRead, List<ReadingCandidate> candidates, int malformedLineCount)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        if (totalLinesRead < 0) throw new ArgumentOutOfRangeException(nameof(totalLinesRead));

        if (malformedLineCount < 0) throw new ArgumentOutOfRangeException(nameof(malformedLineCount));

        TotalLinesRead = totalLinesRead;
        Candidates = candidates;
        MalformedLineCount = malformedLineCount;
    }

    public int TotalLinesRead { get; }

    public List<ReadingCandidate> Candidates { get; }

    public int MalformedLineCount { get; }

    public int ParsedCount => Candidates.Count;
}