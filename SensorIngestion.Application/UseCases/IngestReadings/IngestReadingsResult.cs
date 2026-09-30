namespace SensorIngestion.Application.UseCases.IngestReadings;

public class IngestReadingsResult
{
    public IngestReadingsResult(int totalLinesRead,
        int parsedReadings,
        int malformedLines,
        int invalidRecordsRejected,
        int duplicatesRemoved,
        int storedReadings,
        int newlyInsertedReadings)
    {
        TotalLinesRead = totalLinesRead;
        ParsedReadings = parsedReadings;
        MalformedLines = malformedLines;
        InvalidRecordsRejected = invalidRecordsRejected;
        DuplicatesRemoved = duplicatesRemoved;
        StoredReadings = storedReadings;
        NewlyInsertedReadings = newlyInsertedReadings;
    }

    public int TotalLinesRead { get; }

    public int ParsedReadings { get; }

    public int MalformedLines { get; }

    public int InvalidRecordsRejected { get; }

    public int DuplicatesRemoved { get; }

    public int StoredReadings { get; }

    public int NewlyInsertedReadings { get; }
}