using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Mappings;
using SensorIngestion.Application.Options;
using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Validation.Abstractions;
using SensorIngestion.Domain.ValueObjects;

namespace SensorIngestion.Application.UseCases.IngestReadings;

public class IngestReadingsUseCase
{
    private readonly IReadingsSource _readingsSource;
    private readonly IReadingSemanticValidator _semanticValidator;
    private readonly IReadingRepository _readingRepository;
    private readonly ProcessingOptions _processingOptions;
    private readonly ILogger<IngestReadingsUseCase> _logger;

    public IngestReadingsUseCase(IReadingsSource readingsSource,
        IReadingSemanticValidator semanticValidator,
        IReadingRepository readingRepository,
        IOptions<ProcessingOptions> processingOptions,
        ILogger<IngestReadingsUseCase> logger)
    {
        _readingsSource = readingsSource ?? throw new ArgumentNullException(nameof(readingsSource));
        _semanticValidator = semanticValidator ?? throw new ArgumentNullException(nameof(semanticValidator));
        _readingRepository = readingRepository ?? throw new ArgumentNullException(nameof(readingRepository));
        ArgumentNullException.ThrowIfNull(processingOptions);
        _processingOptions = processingOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IngestReadingsResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_processingOptions.BatchSize <= 0)
            throw new InvalidOperationException("Processing:BatchSize must be a positive integer.");

        var batchSize = _processingOptions.BatchSize;
        var seenKeys = new HashSet<ReadingNaturalKey>();

        var totalLinesRead = 0;
        var parsedReadings = 0;
        var malformedLines = 0;
        var invalidRecordsRejected = 0;
        var duplicatesRemoved = 0;
        var storedReadings = 0;
        var newlyInserted = 0;

        await using var reader = await _readingsSource.OpenBatchesAsync(batchSize, cancellationToken);

        while (true)
        {
            var batch = await reader.ReadNextAsync(cancellationToken);
            if (batch is null)
                break;

            totalLinesRead += batch.LinesRead;
            parsedReadings += batch.ParsedCount;
            malformedLines += batch.MalformedLineCount;

            var batchEntities = new List<SensorReading>(batch.Candidates.Count);

            foreach (var candidate in batch.Candidates)
            {
                var validation = _semanticValidator.Validate(candidate);
                if (!validation.IsValid)
                {
                    invalidRecordsRejected++;
                    _logger.LogDebug(
                        "Rejected invalid reading: {Errors}",
                        string.Join("; ", validation.Errors.Select(e => e.Message)));
                    continue;
                }

                var key = ReadingCandidateMapper.ToNaturalKey(candidate);
                if (!seenKeys.Add(key))
                {
                    duplicatesRemoved++;
                    continue;
                }

                batchEntities.Add(ReadingCandidateMapper.ToEntity(candidate));
            }

            storedReadings += batchEntities.Count;
            newlyInserted += await _readingRepository.InsertNewAsync(batchEntities, cancellationToken);
        }

        _logger.LogInformation("Ingest finished. Lines={Lines}, Parsed={Parsed}, Malformed={Malformed}, Invalid={Invalid}, Duplicates={Duplicates}, Unique={Unique}, Inserted={Inserted}, BatchSize={BatchSize}.",
            totalLinesRead,
            parsedReadings,
            malformedLines,
            invalidRecordsRejected,
            duplicatesRemoved,
            storedReadings,
            newlyInserted,
            batchSize);

        return new IngestReadingsResult(totalLinesRead: totalLinesRead,
            parsedReadings: parsedReadings,
            malformedLines: malformedLines,
            invalidRecordsRejected: invalidRecordsRejected,
            duplicatesRemoved: duplicatesRemoved,
            storedReadings: storedReadings,
            newlyInsertedReadings: newlyInserted);
    }
}