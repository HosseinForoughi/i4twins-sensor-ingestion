using SensorIngestion.Domain.Alerting.Implementations;
using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Tests.Unit.Domain.Alerting;

public class AlertCooldownFilterTests
{
    private readonly AlertCooldownFilter _sut = new();
    private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(5);

    [Fact]
    public void Filter_WhenEpisodeStartsInsideCooldown_SuppressesEpisode()
    {
        // Arrange
        var existing = new List<Alert>
        {
            new(
                ruleId: "r1",
                deviceId: "PUMP-01",
                metric: "temperature",
                startTs: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
                endTs: DateTimeOffset.Parse("2025-06-01T08:01:00Z"),
                peakValue: 75)
        };

        var episodes = new List<SustainedEpisode>
        {
            new(
                ruleId: "r1",
                deviceId: "PUMP-01",
                metric: "temperature",
                startTs: DateTimeOffset.Parse("2025-06-01T08:03:00Z"),
                endTs: DateTimeOffset.Parse("2025-06-01T08:04:00Z"),
                peakValue: 76)
        };

        // Act
        var result = _sut.Filter(episodes, existing, Cooldown);

        // Assert
        Assert.Empty(result.Accepted);
        Assert.Single(result.Suppressed);
    }

    [Fact]
    public void Filter_WhenEpisodeStartsAfterCooldown_AcceptsEpisode()
    {
        // Arrange
        var existing = new List<Alert>
        {
            new(
                ruleId: "r1",
                deviceId: "PUMP-01",
                metric: "temperature",
                startTs: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
                endTs: DateTimeOffset.Parse("2025-06-01T08:01:00Z"),
                peakValue: 75)
        };

        var episodes = new List<SustainedEpisode>
        {
            new(
                ruleId: "r1",
                deviceId: "PUMP-01",
                metric: "temperature",
                startTs: DateTimeOffset.Parse("2025-06-01T08:06:00Z"),
                endTs: DateTimeOffset.Parse("2025-06-01T08:07:00Z"),
                peakValue: 76)
        };

        // Act
        var result = _sut.Filter(episodes, existing, Cooldown);

        // Assert
        Assert.Single(result.Accepted);
        Assert.Empty(result.Suppressed);
    }

    [Fact]
    public void Filter_WhenTwoEpisodesInBatchWithinCooldown_AcceptsOnlyFirst()
    {
        // Arrange
        var episodes = new List<SustainedEpisode>
        {
            new(
                ruleId: "r1",
                deviceId: "PUMP-01",
                metric: "temperature",
                startTs: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
                endTs: DateTimeOffset.Parse("2025-06-01T08:01:00Z"),
                peakValue: 75),
            new(
                ruleId: "r1",
                deviceId: "PUMP-01",
                metric: "temperature",
                startTs: DateTimeOffset.Parse("2025-06-01T08:02:00Z"),
                endTs: DateTimeOffset.Parse("2025-06-01T08:03:00Z"),
                peakValue: 76)
        };

        // Act
        var result = _sut.Filter(episodes, [], Cooldown);

        // Assert
        Assert.Single(result.Accepted);
        Assert.Single(result.Suppressed);
    }
}