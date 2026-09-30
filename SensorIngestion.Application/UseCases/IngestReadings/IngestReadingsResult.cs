namespace SensorIngestion.Application.UseCases.IngestReadings;

/// <summary>
/// Ingest stage counts for one pipeline run.
/// </summary>
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

    /// <summary>
    /// Total lines consumed from the JSONL file (including blank/malformed).
    /// </summary>
    /// <example>2150</example>
    public int TotalLinesRead { get; }

    /// <summary>
    /// Lines that parsed as JSON objects into reading candidates.
    /// </summary>
    public int ParsedReadings { get; }

    /// <summary>
    /// Blank lines or lines that were not valid JSON objects.
    /// </summary>
    public int MalformedLines { get; }

    /// <summary>
    /// Parsed candidates rejected by semantic validation.
    /// </summary>
    public int InvalidRecordsRejected { get; }

    /// <summary>
    /// Valid candidates skipped by first-wins natural-key deduplication.
    /// </summary>
    public int DuplicatesRemoved { get; }

    /// <summary>
    /// Unique valid readings prepared for persistence in this run.
    /// </summary>
    public int StoredReadings { get; }

    /// <summary>
    /// Rows actually inserted into the database (0 on an idempotent re-run).
    /// </summary>
    public int NewlyInsertedReadings { get; }
}