namespace SensorIngestion.Domain.Enums;

public enum RuleOperator : byte
{
    GreaterThan = 1,
    GreaterThanOrEqual = 2,
    LessThan = 4,
    LessThanOrEqual = 8,
    Equal = 16,
    Between = 32,
    SustainedAbove = 64
}