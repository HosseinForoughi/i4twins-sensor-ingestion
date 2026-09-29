using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Domain.SustainedAbove.Abstractions;

public interface ISustainedAboveEvaluator
{
    SustainedAboveEvaluationResult Evaluate(Rule rule, List<SensorReading> readings);
}