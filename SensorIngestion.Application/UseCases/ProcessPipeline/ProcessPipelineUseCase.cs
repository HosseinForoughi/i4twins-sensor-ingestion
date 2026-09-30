using SensorIngestion.Application.UseCases.EvaluateRules;
using SensorIngestion.Application.UseCases.EvaluateSustainedAbove;
using SensorIngestion.Application.UseCases.IngestReadings;

namespace SensorIngestion.Application.UseCases.ProcessPipeline;

public class ProcessPipelineUseCase
{
    private readonly IngestReadingsUseCase _ingestReadings;
    private readonly EvaluateInstantaneousRulesUseCase _evaluateInstantaneousRules;
    private readonly EvaluateSustainedAboveUseCase _evaluateSustainedAbove;

    public ProcessPipelineUseCase(IngestReadingsUseCase ingestReadings,
        EvaluateInstantaneousRulesUseCase evaluateInstantaneousRules,
        EvaluateSustainedAboveUseCase evaluateSustainedAbove)
    {
        _ingestReadings = ingestReadings ?? throw new ArgumentNullException(nameof(ingestReadings));
        _evaluateInstantaneousRules = evaluateInstantaneousRules ?? throw new ArgumentNullException(nameof(evaluateInstantaneousRules));
        _evaluateSustainedAbove = evaluateSustainedAbove ?? throw new ArgumentNullException(nameof(evaluateSustainedAbove));
    }

    public async Task<ProcessPipelineResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var ingest = await _ingestReadings.ExecuteAsync(cancellationToken);
        var instantaneous = await _evaluateInstantaneousRules.ExecuteAsync(cancellationToken);
        var sustained = await _evaluateSustainedAbove.ExecuteAsync(cancellationToken);

        return new ProcessPipelineResult(ingest, instantaneous, sustained);
    }
}