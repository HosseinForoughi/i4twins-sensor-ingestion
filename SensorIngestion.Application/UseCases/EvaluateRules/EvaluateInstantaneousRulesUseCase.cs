using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Options;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.Rules.Abstractions;

namespace SensorIngestion.Application.UseCases.EvaluateRules;

public class EvaluateInstantaneousRulesUseCase
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IReadingRepository _readingRepository;
    private readonly IInstantaneousRuleEngine _ruleEngine;
    private readonly ProcessingOptions _processingOptions;
    private readonly ILogger<EvaluateInstantaneousRulesUseCase> _logger;

    public EvaluateInstantaneousRulesUseCase(IRuleRepository ruleRepository,
        IReadingRepository readingRepository,
        IInstantaneousRuleEngine ruleEngine,
        IOptions<ProcessingOptions> processingOptions,
        ILogger<EvaluateInstantaneousRulesUseCase> logger)
    {
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _readingRepository = readingRepository ?? throw new ArgumentNullException(nameof(readingRepository));
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        ArgumentNullException.ThrowIfNull(processingOptions);
        _processingOptions = processingOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EvaluateInstantaneousRulesResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_processingOptions.BatchSize <= 0)
            throw new InvalidOperationException("Processing:BatchSize must be a positive integer.");

        var allRules = await _ruleRepository.ListAsync(cancellationToken);
        var enabledInstantaneousRules = allRules
            .Where(r => r.Enabled && !r.IsStateful)
            .ToList();

        var batchSize = _processingOptions.BatchSize;
        var readingsEvaluated = 0;
        var acceptable = 0;
        var unacceptable = 0;
        var violations = 0;

        while (true)
        {
            var readings = await _readingRepository.ListUnprocessedBatchAsync(batchSize, cancellationToken);
            if (readings.Count == 0)
                break;

            foreach (var reading in readings)
            {
                _ruleEngine.Evaluate(reading, enabledInstantaneousRules);

                if (reading.Classification == ReadingClassification.Acceptable)
                {
                    acceptable++;
                }
                else if (reading.Classification == ReadingClassification.Unacceptable)
                {
                    unacceptable++;
                    violations += reading.Violations.Count;
                }
            }

            await _readingRepository.SaveChangesAsync(cancellationToken);
            _readingRepository.ClearTracking();
            readingsEvaluated += readings.Count;
        }

        _logger.LogInformation("Instantaneous rule evaluation finished. Rules={Rules}, Evaluated={Evaluated}, Acceptable={Acceptable}, Unacceptable={Unacceptable}, Violations={Violations}, BatchSize={BatchSize}.",
            enabledInstantaneousRules.Count,
            readingsEvaluated,
            acceptable,
            unacceptable,
            violations,
            batchSize);

        return new EvaluateInstantaneousRulesResult(rulesLoaded: enabledInstantaneousRules.Count,
            readingsEvaluated: readingsEvaluated,
            acceptableReadings: acceptable,
            unacceptableReadings: unacceptable,
            ruleViolations: violations);
    }
}