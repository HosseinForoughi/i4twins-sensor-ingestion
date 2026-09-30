using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Options;
using SensorIngestion.Application.Readings;
using SensorIngestion.Application.UseCases.IngestReadings;
using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Validation.Data;
using SensorIngestion.Domain.Validation.Implementations;
using SensorIngestion.Domain.Validation.Implementations.Rules;

namespace SensorIngestion.Tests.Unit.Application;

public class IngestReadingsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenDuplicateAndInvalidCandidates_PersistsOnlyUniqueValidReadings()
    {
        // Arrange
        var batch = new ReadingsBatch(
            candidates: new List<ReadingCandidate>
            {
                Valid(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:00:00Z", seq: 1, value: 70),
                Valid(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:00:00Z", seq: 1, value: 70),
                new ReadingCandidate
                {
                    DeviceId = "",
                    Metric = "temperature",
                    Timestamp = "2025-06-01T08:00:00Z",
                    Sequence = 2,
                    Value = 71
                },
                Valid(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:01:00Z", seq: 3, value: 72)
            },
            linesRead: 5,
            malformedLineCount: 1);

        var readingsSource = CreateSource(batch);

        List<SensorReading>? persisted = null;
        var readingRepository = new Mock<IReadingRepository>();
        readingRepository
            .Setup(r => r.InsertNewAsync(It.IsAny<List<SensorReading>>(), It.IsAny<CancellationToken>()))
            .Callback<List<SensorReading>, CancellationToken>((readings, _) => persisted = readings)
            .ReturnsAsync(2);

        var sut = new IngestReadingsUseCase(
            readingsSource.Object,
            new ReadingSemanticValidator(
            [
                new DeviceIdRequiredRule(),
                new MetricRequiredRule(),
                new SequenceRequiredRule(),
                new ValueFiniteRule(),
                new TimestampValidRule()
            ]),
            readingRepository.Object,
            Options.Create(new ProcessingOptions { BatchSize = 500 }),
            NullLogger<IngestReadingsUseCase>.Instance);

        // Act
        var result = await sut.ExecuteAsync();

        // Assert
        Assert.Equal(5, result.TotalLinesRead);
        Assert.Equal(1, result.MalformedLines);
        Assert.Equal(1, result.InvalidRecordsRejected);
        Assert.Equal(1, result.DuplicatesRemoved);
        Assert.Equal(2, result.StoredReadings);
        Assert.NotNull(persisted);
        Assert.Equal(2, persisted!.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCandidatesSpanMultipleBatches_DedupesAcrossBatchesAndInsertsPerBatch()
    {
        // Arrange
        var batch1 = new ReadingsBatch(
            candidates:
            [
                Valid(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:00:00Z", seq: 1, value: 70)
            ],
            linesRead: 1,
            malformedLineCount: 0);

        var batch2 = new ReadingsBatch(
            candidates:
            [
                Valid(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:00:00Z", seq: 1, value: 99),
                Valid(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:01:00Z", seq: 2, value: 71)
            ],
            linesRead: 2,
            malformedLineCount: 0);

        var readingsSource = CreateSource(batch1, batch2);

        var insertCalls = new List<int>();
        var readingRepository = new Mock<IReadingRepository>();
        readingRepository
            .Setup(r => r.InsertNewAsync(It.IsAny<List<SensorReading>>(), It.IsAny<CancellationToken>()))
            .Callback<List<SensorReading>, CancellationToken>((readings, _) => insertCalls.Add(readings.Count))
            .ReturnsAsync((List<SensorReading> readings, CancellationToken _) => readings.Count);

        var sut = new IngestReadingsUseCase(
            readingsSource.Object,
            new ReadingSemanticValidator(
            [
                new DeviceIdRequiredRule(),
                new MetricRequiredRule(),
                new SequenceRequiredRule(),
                new ValueFiniteRule(),
                new TimestampValidRule()
            ]),
            readingRepository.Object,
            Options.Create(new ProcessingOptions { BatchSize = 1 }),
            NullLogger<IngestReadingsUseCase>.Instance);

        // Act
        var result = await sut.ExecuteAsync();

        // Assert
        Assert.Equal(1, result.DuplicatesRemoved);
        Assert.Equal(2, result.StoredReadings);
        Assert.Equal(2, result.NewlyInsertedReadings);
        Assert.Equal([1, 1], insertCalls);
    }

    private static Mock<IReadingsSource> CreateSource(params ReadingsBatch[] batches)
    {
        var queue = new Queue<ReadingsBatch>(batches);
        var reader = new Mock<IReadingsBatchReader>();
        reader
            .Setup(r => r.ReadNextAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken _) =>
                Task.FromResult<ReadingsBatch?>(queue.Count > 0 ? queue.Dequeue() : null));
        reader
            .Setup(r => r.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        var source = new Mock<IReadingsSource>();
        source
            .Setup(s => s.OpenBatchesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reader.Object);

        return source;
    }

    private static ReadingCandidate Valid(
        string deviceId,
        string metric,
        string ts,
        long seq,
        double value) =>
        new()
        {
            DeviceId = deviceId,
            Metric = metric,
            Timestamp = ts,
            Sequence = seq,
            Value = value
        };
}