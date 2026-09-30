using Microsoft.Extensions.Logging;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.Rules.Abstractions;

namespace SensorIngestion.Application.UseCases.EvaluateRules;

public class EvaluateInstantaneousRulesUseCase
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IReadingRepository _readingRepository;
    private readonly IInstantaneousRuleEngine _ruleEngine;
    private readonly ILogger<EvaluateInstantaneousRulesUseCase> _logger;

    public EvaluateInstantaneousRulesUseCase(IRuleRepository ruleRepository,
        IReadingRepository readingRepository,
        IInstantaneousRuleEngine ruleEngine,
        ILogger<EvaluateInstantaneousRulesUseCase> logger)
    {
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _readingRepository = readingRepository ?? throw new ArgumentNullException(nameof(readingRepository));
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EvaluateInstantaneousRulesResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var allRules = await _ruleRepository.ListAsync(cancellationToken);
        var enabledInstantaneousRules = allRules
            .Where(r => r.Enabled && !r.IsStateful)
            .ToList();

        var readings = await _readingRepository.ListUnprocessedAsync(cancellationToken);

        var acceptable = 0;
        var unacceptable = 0;
        var violations = 0;

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

        _logger.LogInformation("Instantaneous rule evaluation finished. Rules={Rules}, Evaluated={Evaluated}, Acceptable={Acceptable}, Unacceptable={Unacceptable}, Violations={Violations}.",
            enabledInstantaneousRules.Count,
            readings.Count,
            acceptable,
            unacceptable,
            violations);

        return new EvaluateInstantaneousRulesResult(rulesLoaded: enabledInstantaneousRules.Count,
            readingsEvaluated: readings.Count,
            acceptableReadings: acceptable,
            unacceptableReadings: unacceptable,
            ruleViolations: violations);
    }
}