using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.Operators.Implementations;

namespace SensorIngestion.Tests.Unit.Domain.Operators;

public class InstantaneousOperatorEvaluatorTests
{
    [Theory]
    [InlineData(81, 80, true)]
    [InlineData(80, 80, false)]
    [InlineData(79, 80, false)]
    public void GreaterThan_Evaluate_WhenComparedToThreshold_ReturnsExpectedViolation(
        double value,
        double threshold,
        bool expectedViolated)
    {
        // Arrange
        var sut = new GreaterThanOperatorEvaluator();
        var rule = CreateRule(@operator: RuleOperator.GreaterThan, threshold: threshold);

        // Act
        var result = sut.Evaluate(rule, value);

        // Assert
        Assert.Equal(expectedViolated, result.IsViolated);
    }

    [Theory]
    [InlineData(80, 80, true)]
    [InlineData(79, 80, false)]
    public void GreaterThanOrEqual_Evaluate_WhenComparedToThreshold_ReturnsExpectedViolation(
        double value,
        double threshold,
        bool expectedViolated)
    {
        // Arrange
        var sut = new GreaterThanOrEqualOperatorEvaluator();
        var rule = CreateRule(@operator: RuleOperator.GreaterThanOrEqual, threshold: threshold);

        // Act
        var result = sut.Evaluate(rule, value);

        // Assert
        Assert.Equal(expectedViolated, result.IsViolated);
    }

    [Theory]
    [InlineData(5, 5, 7, true)]
    [InlineData(4.9, 5, 7, false)]
    [InlineData(7.1, 5, 7, false)]
    public void Between_Evaluate_WhenValueInsideInclusiveRange_ReturnsExpectedViolation(
        double value,
        double min,
        double max,
        bool expectedViolated)
    {
        // Arrange
        var sut = new BetweenOperatorEvaluator();
        var rule = new Rule(
            ruleId: "band",
            name: "Band",
            enabled: true,
            metric: "vibration",
            @operator: RuleOperator.Between,
            minValue: min,
            maxValue: max);

        // Act
        var result = sut.Evaluate(rule, value);

        // Assert
        Assert.Equal(expectedViolated, result.IsViolated);
    }

    private static Rule CreateRule(RuleOperator @operator, double threshold) =>
        new(ruleId: "r1",
            name: "Rule",
            enabled: true,
            metric: "temperature",
            @operator: @operator,
            threshold: threshold);
}