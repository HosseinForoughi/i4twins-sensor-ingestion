using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Readings;
using SensorIngestion.Application.UseCases.IngestReadings;
using SensorIngestion.Domain.Deduplication.Implementations;
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
        var loadResult = new ReadingsLoadResult(
            totalLinesRead: 4,
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
            malformedLineCount: 1);

        var readingsSource = new Mock<IReadingsSource>();
        readingsSource
            .Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(loadResult);

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
            new FirstWinsReadingDeduplicator(),
            readingRepository.Object,
            NullLogger<IngestReadingsUseCase>.Instance);

        // Act
        var result = await sut.ExecuteAsync();

        // Assert
        Assert.Equal(4, result.TotalLinesRead);
        Assert.Equal(1, result.MalformedLines);
        Assert.Equal(1, result.InvalidRecordsRejected);
        Assert.Equal(1, result.DuplicatesRemoved);
        Assert.Equal(2, result.StoredReadings);
        Assert.NotNull(persisted);
        Assert.Equal(2, persisted!.Count);
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