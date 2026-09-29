using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Domain.Alerting.Models;

public class AlertCooldownFilterResult
{
    public AlertCooldownFilterResult(List<SustainedEpisode> accepted, List<SustainedEpisode> suppressed)
    {
        ArgumentNullException.ThrowIfNull(accepted);
        ArgumentNullException.ThrowIfNull(suppressed);

        Accepted = accepted;
        Suppressed = suppressed;
    }

    public List<SustainedEpisode> Accepted { get; }

    public List<SustainedEpisode> Suppressed { get; }

    public int AcceptedCount => Accepted.Count;

    public int SuppressedCount => Suppressed.Count;
}
