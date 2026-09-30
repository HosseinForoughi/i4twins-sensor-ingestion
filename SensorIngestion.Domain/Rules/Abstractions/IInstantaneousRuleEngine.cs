using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Domain.Rules.Abstractions;

public interface IInstantaneousRuleEngine
{
    void Evaluate(SensorReading reading, List<Rule> rules);
}