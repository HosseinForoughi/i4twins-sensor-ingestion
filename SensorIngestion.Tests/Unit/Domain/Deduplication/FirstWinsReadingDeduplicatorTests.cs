using SensorIngestion.Domain.Deduplication.Implementations;
using SensorIngestion.Domain.Validation.Data;
using SensorIngestion.Domain.ValueObjects;

namespace SensorIngestion.Tests.Unit.Domain.Deduplication;

public class FirstWinsReadingDeduplicatorTests
{
    private readonly FirstWinsReadingDeduplicator _sut = new();

    [Fact]
    public void Deduplicate_WhenDuplicateNaturalKeys_KeepsFirstOccurrence()
    {
        // Arrange
        var first = CreateCandidate(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:00:00Z", seq: 1, value: 10);
        var duplicate = CreateCandidate(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:00:00Z", seq: 1, value: 99);
        var other = CreateCandidate(deviceId: "PUMP-01", metric: "temperature", ts: "2025-06-01T08:01:00Z", seq: 2, value: 11);

        // Act
        var result = _sut.Deduplicate(
            [first, duplicate, other],
            c => new ReadingNaturalKey(
                DeviceId: c.DeviceId!,
                Metric: c.Metric!,
                Timestamp: DateTimeOffset.Parse(c.Timestamp!),
                Sequence: c.Sequence!.Value));

        // Assert
        Assert.Equal(2, result.UniqueCount);
        Assert.Equal(1, result.DuplicatesRemovedCount);
        Assert.Same(first, result.Unique[0]);
        Assert.Same(duplicate, result.Duplicates[0]);
    }

    [Fact]
    public void Deduplicate_WhenAllUnique_ReturnsNoDuplicates()
    {
        // Arrange
        var items = new List<ReadingCandidate>
        {
            CreateCandidate(deviceId: "A", metric: "m", ts: "2025-06-01T08:00:00Z", seq: 1, value: 1),
            CreateCandidate(deviceId: "A", metric: "m", ts: "2025-06-01T08:00:01Z", seq: 2, value: 2)
        };

        // Act
        var result = _sut.Deduplicate(
            items,
            c => new ReadingNaturalKey(
                DeviceId: c.DeviceId!,
                Metric: c.Metric!,
                Timestamp: DateTimeOffset.Parse(c.Timestamp!),
                Sequence: c.Sequence!.Value));

        // Assert
        Assert.Equal(2, result.UniqueCount);
        Assert.Equal(0, result.DuplicatesRemovedCount);
    }

    private static ReadingCandidate CreateCandidate(
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