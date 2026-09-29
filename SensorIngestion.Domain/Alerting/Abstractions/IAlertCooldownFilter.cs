using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Alerting.Models;
using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Domain.Alerting.Abstractions;

public interface IAlertCooldownFilter
{
    AlertCooldownFilterResult Filter(List<SustainedEpisode> qualifyingEpisodes, List<Alert> existingAlerts, TimeSpan cooldown);
}