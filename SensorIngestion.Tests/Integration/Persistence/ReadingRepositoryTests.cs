using SensorIngestion.Domain.Entities;
using SensorIngestion.Infrastructure.Persistence.Repositories;
using SensorIngestion.Tests.Integration.Fixtures;

namespace SensorIngestion.Tests.Integration.Persistence;

[Collection(PostgreSqlCollection.Name)]
public class ReadingRepositoryTests
{
    private readonly PostgreSqlFixture _fixture;

    public ReadingRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InsertNewAsync_WhenSameNaturalKeyInsertedTwice_InsertsOnlyOnce()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await using var db = _fixture.CreateDbContext();
        var sut = new ReadingRepository(db);

        var reading = new SensorReading(
            deviceId: "PUMP-01",
            metric: "temperature",
            timestamp: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
            sequence: 1,
            value: 70);

        // Act
        var first = await sut.InsertNewAsync([reading], CancellationToken.None);
        var second = await sut.InsertNewAsync(
        [
            new SensorReading(
                deviceId: "PUMP-01",
                metric: "temperature",
                timestamp: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
                sequence: 1,
                value: 99)
        ],
        CancellationToken.None);

        // Assert
        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Equal(1, await sut.CountAsync());
    }
}