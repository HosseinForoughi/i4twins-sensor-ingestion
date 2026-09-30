using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Domain.SustainedAbove.Abstractions;

public interface ISustainedAboveViolationApplier
{
    int Apply(Rule rule, List<SustainedEpisode> episodes, List<SensorReading> readings);
}