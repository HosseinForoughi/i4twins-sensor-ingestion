using SensorIngestion.Application.UseCases.GetAggregates;
using SensorIngestion.Domain.Entities;
using SensorIngestion.Infrastructure.Persistence.Queries;
using SensorIngestion.Tests.Integration.Fixtures;

namespace SensorIngestion.Tests.Integration.Persistence;

[Collection(PostgreSqlCollection.Name)]
public class ReadingAggregationQueryTests
{
    private readonly PostgreSqlFixture _fixture;

    public ReadingAggregationQueryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAggregatesAsync_WhenUnacceptableReadingExists_ExcludesItFromBuckets()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var db = _fixture.CreateDbContext();

        var acceptable = new SensorReading(deviceId: "PUMP-01",
            metric: "temperature",
            timestamp: DateTimeOffset.Parse("2025-06-01T08:00:10Z"),
            sequence: 1,
            value: 70);
        acceptable.MarkAsAcceptable();

        var unacceptable = new SensorReading(deviceId: "PUMP-01",
            metric: "temperature",
            timestamp: DateTimeOffset.Parse("2025-06-01T08:00:20Z"),
            sequence: 2,
            value: 90);
        unacceptable.MarkAsUnacceptable(new ReadingViolation(ruleId: "r1", reason: "too hot"));

        db.Readings.AddRange(acceptable, unacceptable);
        await db.SaveChangesAsync();

        var sut = new ReadingAggregationQuery(db);

        // Act
        var result = await sut.GetAggregatesAsync(
            new GetAggregatesRequest(deviceId: "PUMP-01",
                metric: "temperature",
                from: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
                to: DateTimeOffset.Parse("2025-06-01T09:00:00Z"),
                bucketSeconds: 60));

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].Count);
        Assert.Equal(70, result[0].Average);
    }

    [Fact]
    public async Task GetAggregatesAsync_WhenBucketHasNoReadings_OmitsEmptyBucket()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var db = _fixture.CreateDbContext();

        var reading = new SensorReading(deviceId: "PUMP-01",
            metric: "temperature",
            timestamp: DateTimeOffset.Parse("2025-06-01T08:00:10Z"),
            sequence: 1,
            value: 70);
        reading.MarkAsAcceptable();
        db.Readings.Add(reading);
        await db.SaveChangesAsync();

        var sut = new ReadingAggregationQuery(db);

        // Act
        var result = await sut.GetAggregatesAsync(
            new GetAggregatesRequest(deviceId: "PUMP-01",
                metric: "temperature",
                from: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
                to: DateTimeOffset.Parse("2025-06-01T08:10:00Z"),
                bucketSeconds: 60));

        // Assert
        Assert.Single(result);
        Assert.DoesNotContain(result, b => b.BucketStart == DateTimeOffset.Parse("2025-06-01T08:01:00Z"));
    }
}