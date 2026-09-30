using SensorIngestion.Application.UseCases.EvaluateRules;
using SensorIngestion.Application.UseCases.EvaluateSustainedAbove;
using SensorIngestion.Application.UseCases.IngestReadings;

namespace SensorIngestion.Application.UseCases.ProcessPipeline;

public class ProcessPipelineResult
{
    public ProcessPipelineResult(
        IngestReadingsResult ingest,
        EvaluateInstantaneousRulesResult instantaneousRules,
        EvaluateSustainedAboveResult sustainedAbove)
    {
        Ingest = ingest ?? throw new ArgumentNullException(nameof(ingest));
        InstantaneousRules = instantaneousRules
            ?? throw new ArgumentNullException(nameof(instantaneousRules));
        SustainedAbove = sustainedAbove
            ?? throw new ArgumentNullException(nameof(sustainedAbove));
    }

    public IngestReadingsResult Ingest { get; }

    public EvaluateInstantaneousRulesResult InstantaneousRules { get; }

    public EvaluateSustainedAboveResult SustainedAbove { get; }
}
