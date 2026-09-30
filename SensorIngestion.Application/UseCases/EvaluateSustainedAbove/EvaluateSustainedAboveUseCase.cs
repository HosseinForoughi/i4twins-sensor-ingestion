using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Options;
using SensorIngestion.Domain.Alerting.Abstractions;
using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;
using SensorIngestion.Domain.SustainedAbove.Abstractions;
using SensorIngestion.Domain.SustainedAbove.Models;

namespace SensorIngestion.Application.UseCases.EvaluateSustainedAbove;

public class EvaluateSustainedAboveUseCase
{
    private readonly IRuleRepository _ruleRepository;
    private readonly IReadingRepository _readingRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly ISustainedAboveEvaluator _sustainedAboveEvaluator;
    private readonly IAlertCooldownFilter _alertCooldownFilter;
    private readonly ISustainedAboveViolationApplier _violationApplier;
    private readonly AlertingOptions _alertingOptions;
    private readonly ILogger<EvaluateSustainedAboveUseCase> _logger;

    public EvaluateSustainedAboveUseCase(IRuleRepository ruleRepository,
        IReadingRepository readingRepository,
        IAlertRepository alertRepository,
        ISustainedAboveEvaluator sustainedAboveEvaluator,
        IAlertCooldownFilter alertCooldownFilter,
        ISustainedAboveViolationApplier violationApplier,
        IOptions<AlertingOptions> alertingOptions,
        ILogger<EvaluateSustainedAboveUseCase> logger)
    {
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _readingRepository = readingRepository ?? throw new ArgumentNullException(nameof(readingRepository));
        _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
        _sustainedAboveEvaluator = sustainedAboveEvaluator ?? throw new ArgumentNullException(nameof(sustainedAboveEvaluator));
        _alertCooldownFilter = alertCooldownFilter ?? throw new ArgumentNullException(nameof(alertCooldownFilter));
        _violationApplier = violationApplier ?? throw new ArgumentNullException(nameof(violationApplier));

        ArgumentNullException.ThrowIfNull(alertingOptions);
        _alertingOptions = alertingOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EvaluateSustainedAboveResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        if (_alertingOptions.CooldownMinutes <= 0)
            throw new InvalidOperationException("Alerting:CooldownMinutes must be a positive integer.");

        var cooldown = TimeSpan.FromMinutes(_alertingOptions.CooldownMinutes);

        var sustainedRules = (await _ruleRepository.ListAsync(cancellationToken))
            .Where(r => r.Enabled && r.Operator == RuleOperator.SustainedAbove)
            .ToList();

        var readings = await _readingRepository.ListAllTrackedAsync(cancellationToken);
        var existingAlerts = await _alertRepository.ListAsync(cancellationToken);

        var qualifyingEpisodeCount = 0;
        var acceptedEpisodes = new List<SustainedEpisode>();
        var suppressedCount = 0;
        var readingViolationsAdded = 0;

        foreach (var rule in sustainedRules)
        {
            var evaluation = _sustainedAboveEvaluator.Evaluate(rule, readings);
            qualifyingEpisodeCount += evaluation.EpisodeCount;

            readingViolationsAdded += _violationApplier.Apply(
                rule,
                evaluation.QualifyingEpisodes,
                readings);

            var cooldownResult = _alertCooldownFilter.Filter(
                evaluation.QualifyingEpisodes,
                existingAlerts,
                cooldown);

            acceptedEpisodes.AddRange(cooldownResult.Accepted);
            suppressedCount += cooldownResult.SuppressedCount;

            foreach (var episode in cooldownResult.Accepted)
            {
                existingAlerts.Add(
                    new Alert(episode.RuleId, episode.DeviceId,
                        episode.Metric,
                        episode.StartTs,
                        episode.EndTs,
                        episode.PeakValue));
            }
        }

        var alertsToInsert = acceptedEpisodes.Select(e => new Alert(e.RuleId, e.DeviceId, e.Metric, e.StartTs, e.EndTs, e.PeakValue))
            .ToList();

        var inserted = await _alertRepository.InsertNewAsync(alertsToInsert, cancellationToken);
        await _readingRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("SustainedAbove finished. Rules={Rules}, Episodes={Episodes}, AlertsAccepted={Accepted}, Suppressed={Suppressed}, Inserted={Inserted}, ReadingViolations={Violations}.",
            sustainedRules.Count,
            qualifyingEpisodeCount,
            acceptedEpisodes.Count,
            suppressedCount,
            inserted,
            readingViolationsAdded);

        return new EvaluateSustainedAboveResult(rulesEvaluated: sustainedRules.Count,
            qualifyingEpisodes: qualifyingEpisodeCount,
            alertsAccepted: acceptedEpisodes.Count,
            alertsSuppressedByCooldown: suppressedCount,
            alertsInserted: inserted,
            readingViolationsAdded: readingViolationsAdded);
    }
}