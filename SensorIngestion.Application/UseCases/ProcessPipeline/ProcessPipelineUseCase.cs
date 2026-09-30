using Microsoft.Extensions.Logging;
using SensorIngestion.Application.UseCases.EvaluateRules;
using SensorIngestion.Application.UseCases.EvaluateSustainedAbove;
using SensorIngestion.Application.UseCases.IngestReadings;

namespace SensorIngestion.Application.UseCases.ProcessPipeline;

public class ProcessPipelineUseCase
{
    private readonly IngestReadingsUseCase _ingestReadings;
    private readonly EvaluateInstantaneousRulesUseCase _evaluateInstantaneousRules;
    private readonly EvaluateSustainedAboveUseCase _evaluateSustainedAbove;
    private readonly ILogger<ProcessPipelineUseCase> _logger;

    public ProcessPipelineUseCase(IngestReadingsUseCase ingestReadings,
        EvaluateInstantaneousRulesUseCase evaluateInstantaneousRules,
        EvaluateSustainedAboveUseCase evaluateSustainedAbove,
        ILogger<ProcessPipelineUseCase> logger)
    {
        _ingestReadings = ingestReadings ?? throw new ArgumentNullException(nameof(ingestReadings));
        _evaluateInstantaneousRules = evaluateInstantaneousRules ?? throw new ArgumentNullException(nameof(evaluateInstantaneousRules));
        _evaluateSustainedAbove = evaluateSustainedAbove ?? throw new ArgumentNullException(nameof(evaluateSustainedAbove));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProcessPipelineResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var ingest = await _ingestReadings.ExecuteAsync(cancellationToken);
        var instantaneous = await _evaluateInstantaneousRules.ExecuteAsync(cancellationToken);
        var sustained = await _evaluateSustainedAbove.ExecuteAsync(cancellationToken);

        var result = new ProcessPipelineResult(ingest, instantaneous, sustained);
        LogProcessingReport(result);
        return result;
    }

    private void LogProcessingReport(ProcessPipelineResult result)
    {
        _logger.LogInformation(
            "Processing report. TotalLines={TotalLines}, Parsed={Parsed}, Stored={Stored}, DuplicatesRemoved={Duplicates}, InvalidRejected={Invalid}, RulesLoaded={Rules}, Evaluations={Evaluations}, Acceptable={Acceptable}, Unacceptable={Unacceptable}, Violations={Violations}, AlertsGenerated={Alerts}.",
            result.Ingest.TotalLinesRead,
            result.Ingest.ParsedReadings,
            result.Ingest.StoredReadings,
            result.Ingest.DuplicatesRemoved,
            result.Ingest.InvalidRecordsRejected,
            result.InstantaneousRules.RulesLoaded,
            result.InstantaneousRules.ReadingsEvaluated,
            result.InstantaneousRules.AcceptableReadings,
            result.InstantaneousRules.UnacceptableReadings,
            result.InstantaneousRules.RuleViolations + result.SustainedAbove.ReadingViolationsAdded,
            result.SustainedAbove.AlertsInserted);
    }
}