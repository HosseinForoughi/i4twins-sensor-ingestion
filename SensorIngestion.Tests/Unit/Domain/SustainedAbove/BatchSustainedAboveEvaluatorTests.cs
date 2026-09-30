using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.SustainedAbove.Implementations;

namespace SensorIngestion.Tests.Unit.Domain.SustainedAbove;

public class BatchSustainedAboveEvaluatorTests
{
    private readonly BatchSustainedAboveEvaluator _sut = new();

    [Fact]
    public void Evaluate_WhenReadingsAreOutOfOrder_DetectsSameQualifyingEpisode()
    {
        // Arrange
        var rule = new Rule(
            ruleId: "sustained-69",
            name: "Sustained",
            enabled: true,
            metric: "temperature",
            @operator: RuleOperator.SustainedAbove,
            deviceId: "PUMP-01",
            threshold: 69,
            durationSeconds: 30);

        // Intentionally out of chronological file order
        var readings = new List<SensorReading>
        {
            new(
                deviceId: "PUMP-01",
                metric: "temperature",
                timestamp: DateTimeOffset.Parse("2025-06-01T08:00:40Z"),
                sequence: 3,
                value: 70),
            new(
                deviceId: "PUMP-01",
                metric: "temperature",
                timestamp: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
                sequence: 1,
                value: 70),
            new(
                deviceId: "PUMP-01",
                metric: "temperature",
                timestamp: DateTimeOffset.Parse("2025-06-01T08:00:20Z"),
                sequence: 2,
                value: 71),
            new(
                deviceId: "PUMP-01",
                metric: "temperature",
                timestamp: DateTimeOffset.Parse("2025-06-01T08:01:00Z"),
                sequence: 4,
                value: 60)
        };

        // Act
        var result = _sut.Evaluate(rule, readings);

        // Assert
        Assert.Equal(1, result.EpisodeCount);
        var episode = result.QualifyingEpisodes[0];
        Assert.Equal(DateTimeOffset.Parse("2025-06-01T08:00:00Z"), episode.StartTs);
        Assert.Equal(DateTimeOffset.Parse("2025-06-01T08:01:00Z"), episode.EndTs);
        Assert.Equal(71, episode.PeakValue);
    }

    [Fact]
    public void Evaluate_WhenDurationNotMet_ReturnsNoEpisode()
    {
        // Arrange
        var rule = new Rule(
            ruleId: "sustained-69",
            name: "Sustained",
            enabled: true,
            metric: "temperature",
            @operator: RuleOperator.SustainedAbove,
            deviceId: "PUMP-01",
            threshold: 69,
            durationSeconds: 60);

        var readings = new List<SensorReading>
        {
            new(
                deviceId: "PUMP-01",
                metric: "temperature",
                timestamp: DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
                sequence: 1,
                value: 70),
            new(
                deviceId: "PUMP-01",
                metric: "temperature",
                timestamp: DateTimeOffset.Parse("2025-06-01T08:00:30Z"),
                sequence: 2,
                value: 60)
        };

        // Act
        var result = _sut.Evaluate(rule, readings);

        // Assert
        Assert.Equal(0, result.EpisodeCount);
    }
}