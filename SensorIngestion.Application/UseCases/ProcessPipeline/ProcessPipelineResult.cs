using SensorIngestion.Application.UseCases.EvaluateRules;
using SensorIngestion.Application.UseCases.EvaluateSustainedAbove;
using SensorIngestion.Application.UseCases.IngestReadings;

namespace SensorIngestion.Application.UseCases.ProcessPipeline;

/// <summary>
/// Combined processing report returned by <c>POST /api/ingest</c>.
/// </summary>
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

    /// <summary>
    /// Counts from JSONL load, validation, deduplication, and persistence.
    /// </summary>
    public IngestReadingsResult Ingest { get; }

    /// <summary>
    /// Counts from instantaneous (non-stateful) rule evaluation and classification.
    /// </summary>
    public EvaluateInstantaneousRulesResult InstantaneousRules { get; }

    /// <summary>
    /// Counts from SustainedAbove episode detection, cooldown filtering, and alert inserts.
    /// </summary>
    public EvaluateSustainedAboveResult SustainedAbove { get; }
}