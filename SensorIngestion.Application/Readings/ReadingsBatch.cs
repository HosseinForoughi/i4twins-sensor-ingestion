using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Application.Readings;

public class ReadingsBatch
{
    public ReadingsBatch(List<ReadingCandidate> candidates, int linesRead, int malformedLineCount)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        if (linesRead < 0)
            throw new ArgumentOutOfRangeException(nameof(linesRead));

        if (malformedLineCount < 0)
            throw new ArgumentOutOfRangeException(nameof(malformedLineCount));

        Candidates = candidates;
        LinesRead = linesRead;
        MalformedLineCount = malformedLineCount;
    }

    public List<ReadingCandidate> Candidates { get; }

    public int LinesRead { get; }

    public int MalformedLineCount { get; }

    public int ParsedCount => Candidates.Count;
}