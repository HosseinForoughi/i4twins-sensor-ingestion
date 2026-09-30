using Microsoft.Extensions.Logging;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Mappings;
using SensorIngestion.Domain.Deduplication.Abstractions;
using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Validation.Abstractions;
using SensorIngestion.Domain.Validation.Data;

namespace SensorIngestion.Application.UseCases.IngestReadings;

public class IngestReadingsUseCase
{
    private readonly IReadingsSource _readingsSource;
    private readonly IReadingSemanticValidator _semanticValidator;
    private readonly IReadingDeduplicator _deduplicator;
    private readonly IReadingRepository _readingRepository;
    private readonly ILogger<IngestReadingsUseCase> _logger;

    public IngestReadingsUseCase(IReadingsSource readingsSource,
        IReadingSemanticValidator semanticValidator,
        IReadingDeduplicator deduplicator,
        IReadingRepository readingRepository,
        ILogger<IngestReadingsUseCase> logger)
    {
        _readingsSource = readingsSource ?? throw new ArgumentNullException(nameof(readingsSource));
        _semanticValidator = semanticValidator ?? throw new ArgumentNullException(nameof(semanticValidator));
        _deduplicator = deduplicator ?? throw new ArgumentNullException(nameof(deduplicator));
        _readingRepository = readingRepository ?? throw new ArgumentNullException(nameof(readingRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IngestReadingsResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var loadResult = await _readingsSource.LoadAsync(cancellationToken);

        var validCandidates = new List<ReadingCandidate>();
        var invalidRecordsRejected = 0;

        foreach (var candidate in loadResult.Candidates)
        {
            var validation = _semanticValidator.Validate(candidate);
            if (validation.IsValid)
            {
                validCandidates.Add(candidate);
            }
            else
            {
                invalidRecordsRejected++;
                _logger.LogDebug("Rejected invalid reading: {Errors}", string.Join("; ", validation.Errors.Select(e => e.Message)));
            }
        }

        var dedupeResult = _deduplicator.Deduplicate(validCandidates, ReadingCandidateMapper.ToNaturalKey);

        var readings = new List<SensorReading>(dedupeResult.Unique.Count);
        foreach (var candidate in dedupeResult.Unique)
        {
            readings.Add(ReadingCandidateMapper.ToEntity(candidate));
        }

        var newlyInserted = await _readingRepository.InsertNewAsync(readings, cancellationToken);

        _logger.LogInformation(
            "Ingest finished. Lines={Lines}, Parsed={Parsed}, Malformed={Malformed}, Invalid={Invalid}, Duplicates={Duplicates}, Unique={Unique}, Inserted={Inserted}.",
            loadResult.TotalLinesRead,
            loadResult.ParsedCount,
            loadResult.MalformedLineCount,
            invalidRecordsRejected,
            dedupeResult.DuplicatesRemovedCount,
            readings.Count,
            newlyInserted);

        return new IngestReadingsResult(totalLinesRead: loadResult.TotalLinesRead,
            parsedReadings: loadResult.ParsedCount,
            malformedLines: loadResult.MalformedLineCount,
            invalidRecordsRejected: invalidRecordsRejected,
            duplicatesRemoved: dedupeResult.DuplicatesRemovedCount,
            storedReadings: readings.Count,
            newlyInsertedReadings: newlyInserted);
    }
}