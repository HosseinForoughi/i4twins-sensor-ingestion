namespace SensorIngestion.Domain.SustainedAbove.Models;

public class SustainedAboveEvaluationResult
{
    public SustainedAboveEvaluationResult(List<SustainedEpisode> qualifyingEpisodes)
    {
        ArgumentNullException.ThrowIfNull(qualifyingEpisodes);
        QualifyingEpisodes = qualifyingEpisodes;
    }

    public List<SustainedEpisode> QualifyingEpisodes { get; }

    public int EpisodeCount => QualifyingEpisodes.Count;
}