using SensorIngestion.Domain.Validation.Data;
using SensorIngestion.Domain.Validation.Implementations;
using SensorIngestion.Domain.Validation.Implementations.Rules;

namespace SensorIngestion.Tests.Unit.Domain.Validation;

public class ReadingSemanticValidatorTests
{
    private readonly ReadingSemanticValidator _sut = new(
    [
        new DeviceIdRequiredRule(),
        new MetricRequiredRule(),
        new SequenceRequiredRule(),
        new ValueFiniteRule(),
        new TimestampValidRule()
    ]);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenDeviceIdMissing_ReturnsInvalid(string? deviceId)
    {
        // Arrange
        var candidate = ValidCandidate() with { DeviceId = deviceId };

        // Act
        var result = _sut.Validate(ToCandidate(candidate));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == DeviceIdRequiredRule.ErrorCode);
    }

    [Fact]
    public void Validate_WhenMetricNull_ReturnsInvalid()
    {
        // Arrange
        var candidate = ValidCandidate() with { Metric = null };

        // Act
        var result = _sut.Validate(ToCandidate(candidate));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == MetricRequiredRule.ErrorCode);
    }

    [Fact]
    public void Validate_WhenSequenceNull_ReturnsInvalid()
    {
        // Arrange
        var candidate = ValidCandidate() with { Sequence = null };

        // Act
        var result = _sut.Validate(ToCandidate(candidate));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == SequenceRequiredRule.ErrorCode);
    }

    [Fact]
    public void Validate_WhenValueIsNaNString_ReturnsInvalid()
    {
        // Arrange
        var candidate = ValidCandidate() with { Value = null, ValueRaw = "NaN" };

        // Act
        var result = _sut.Validate(ToCandidate(candidate));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == ValueFiniteRule.ErrorCode);
    }

    [Fact]
    public void Validate_WhenCalendarDateInvalid_ReturnsInvalid()
    {
        // Arrange
        var candidate = ValidCandidate() with { Timestamp = "2025-06-31T08:00:00Z" };

        // Act
        var result = _sut.Validate(ToCandidate(candidate));

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Code == TimestampValidRule.ErrorCode);
    }

    [Fact]
    public void Validate_WhenCandidateIsValid_ReturnsValid()
    {
        // Arrange
        var candidate = ToCandidate(ValidCandidate());

        // Act
        var result = _sut.Validate(candidate);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    private static CandidateData ValidCandidate() => new(
        DeviceId: "PUMP-01",
        Metric: "temperature",
        Timestamp: "2025-06-01T08:00:00Z",
        Value: 70,
        ValueRaw: null,
        Sequence: 1);

    private static ReadingCandidate ToCandidate(CandidateData data) => new()
    {
        DeviceId = data.DeviceId,
        Metric = data.Metric,
        Timestamp = data.Timestamp,
        Value = data.Value,
        ValueRaw = data.ValueRaw,
        Sequence = data.Sequence
    };

    private record CandidateData(string? DeviceId,
        string? Metric,
        string? Timestamp,
        double? Value,
        string? ValueRaw,
        long? Sequence);
}