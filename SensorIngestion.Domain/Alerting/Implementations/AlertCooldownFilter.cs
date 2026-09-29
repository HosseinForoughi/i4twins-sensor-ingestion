using SensorIngestion.Domain.Alerting.Abstractions;
using SensorIngestion.Domain.Alerting.Models;
using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Domain.Alerting.Implementations;

public class AlertCooldownFilter : IAlertCooldownFilter
{
    public AlertCooldownFilterResult Filter(List<SustainedEpisode> qualifyingEpisodes, List<Alert> existingAlerts, TimeSpan cooldown)
    {
        ArgumentNullException.ThrowIfNull(qualifyingEpisodes);
        ArgumentNullException.ThrowIfNull(existingAlerts);

        if (cooldown <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(cooldown), "Cooldown must be positive.");

        var priorAlerts = existingAlerts.ToList();
        var accepted = new List<SustainedEpisode>();
        var suppressed = new List<SustainedEpisode>();

        var orderedEpisodes = qualifyingEpisodes.OrderBy(e => e.StartTs)
            .ThenBy(e => e.EndTs)
            .ToList();

        foreach (var episode in orderedEpisodes)
        {
            if (IsSuppressedByCooldown(episode, priorAlerts, cooldown))
            {
                suppressed.Add(episode);
                continue;
            }

            accepted.Add(episode);

            priorAlerts.Add(ToAlertPlaceholder(episode));
        }

        return new AlertCooldownFilterResult(accepted, suppressed);
    }

    private static bool IsSuppressedByCooldown(SustainedEpisode episode, List<Alert> priorAlerts, TimeSpan cooldown)
    {
        return priorAlerts.Any(alert =>
            SameAlertKey(alert, episode) &&
            alert.IsInCooldownWindow(episode.StartTs, cooldown));
    }

    private static bool SameAlertKey(Alert alert, SustainedEpisode episode)
    {
        return string.Equals(alert.RuleId, episode.RuleId, StringComparison.Ordinal)
            && string.Equals(alert.DeviceId, episode.DeviceId, StringComparison.Ordinal)
            && string.Equals(alert.Metric, episode.Metric, StringComparison.Ordinal);
    }

    private static Alert ToAlertPlaceholder(SustainedEpisode episode)
    {
        return new(episode.RuleId,
            episode.DeviceId,
            episode.Metric,
            episode.StartTs,
            episode.EndTs,
            episode.PeakValue);
    }
}