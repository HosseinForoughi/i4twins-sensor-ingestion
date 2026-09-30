using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.Operators.Implementations;
using SensorIngestion.Domain.Rules.Implementations;

namespace SensorIngestion.Tests.Unit.Domain.Rules;

public class InstantaneousRuleEngineTests
{
    private readonly InstantaneousRuleEngine _sut = new(
        new OperatorEvaluatorRegistry(
        [
            new GreaterThanOperatorEvaluator(),
            new GreaterThanOrEqualOperatorEvaluator(),
            new LessThanOperatorEvaluator(),
            new LessThanOrEqualOperatorEvaluator(),
            new EqualOperatorEvaluator(),
            new BetweenOperatorEvaluator()
        ]));

    [Fact]
    public void Evaluate_WhenNoApplicableRules_MarksReadingAcceptable()
    {
        // Arrange
        var reading = new SensorReading(
            deviceId: "PUMP-01",
            metric: "temperature",
            timestamp: DateTimeOffset.UtcNow,
            sequence: 1,
            value: 70);
        var rules = new List<Rule>
        {
            new(
                ruleId: "pressure-high",
                name: "Pressure",
                enabled: true,
                metric: "pressure",
                @operator: RuleOperator.GreaterThan,
                threshold: 10)
        };

        // Act
        _sut.Evaluate(reading, rules);

        // Assert
        Assert.Equal(ReadingClassification.Acceptable, reading.Classification);
        Assert.Empty(reading.Violations);
    }

    [Fact]
    public void Evaluate_WhenDeviceScopedRuleDoesNotMatch_IgnoresRule()
    {
        // Arrange
        var reading = new SensorReading(
            deviceId: "PUMP-02",
            metric: "temperature",
            timestamp: DateTimeOffset.UtcNow,
            sequence: 1,
            value: 90);
        var rules = new List<Rule>
        {
            new(
                ruleId: "pump01-only",
                name: "Hot",
                enabled: true,
                metric: "temperature",
                @operator: RuleOperator.GreaterThan,
                deviceId: "PUMP-01",
                threshold: 80)
        };

        // Act
        _sut.Evaluate(reading, rules);

        // Assert
        Assert.Equal(ReadingClassification.Acceptable, reading.Classification);
    }

    [Fact]
    public void Evaluate_WhenRuleViolated_MarksReadingUnacceptableWithReason()
    {
        // Arrange
        var reading = new SensorReading(
            deviceId: "PUMP-01",
            metric: "temperature",
            timestamp: DateTimeOffset.UtcNow,
            sequence: 1,
            value: 90);
        var rules = new List<Rule>
        {
            new(
                ruleId: "temp-gt-80",
                name: "Hot",
                enabled: true,
                metric: "temperature",
                @operator: RuleOperator.GreaterThan,
                threshold: 80)
        };

        // Act
        _sut.Evaluate(reading, rules);

        // Assert
        Assert.Equal(ReadingClassification.Unacceptable, reading.Classification);
        Assert.Single(reading.Violations);
        Assert.Equal("temp-gt-80", reading.Violations.First().RuleId);
    }

    [Fact]
    public void Evaluate_WhenRuleDisabled_IgnoresRule()
    {
        // Arrange
        var reading = new SensorReading(
            deviceId: "PUMP-01",
            metric: "temperature",
            timestamp: DateTimeOffset.UtcNow,
            sequence: 1,
            value: 90);
        var rules = new List<Rule>
        {
            new(
                ruleId: "temp-gt-80",
                name: "Hot",
                enabled: false,
                metric: "temperature",
                @operator: RuleOperator.GreaterThan,
                threshold: 80)
        };

        // Act
        _sut.Evaluate(reading, rules);

        // Assert
        Assert.Equal(ReadingClassification.Acceptable, reading.Classification);
    }
}