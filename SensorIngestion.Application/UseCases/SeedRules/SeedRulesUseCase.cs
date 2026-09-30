using Microsoft.Extensions.Logging;
using SensorIngestion.Application.Abstractions;

namespace SensorIngestion.Application.UseCases.SeedRules;

public class SeedRulesUseCase
{
    private readonly IRulesSeedSource _rulesSeedSource;
    private readonly IRuleRepository _ruleRepository;
    private readonly ILogger<SeedRulesUseCase> _logger;

    public SeedRulesUseCase(IRulesSeedSource rulesSeedSource,
        IRuleRepository ruleRepository,
        ILogger<SeedRulesUseCase> logger)
    {
        _rulesSeedSource = rulesSeedSource ?? throw new ArgumentNullException(nameof(rulesSeedSource));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SeedRulesResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _rulesSeedSource.LoadAsync(cancellationToken);

        _logger.LogInformation("Loaded {Count} rule(s) from seed file.", rules.Count);

        var existing = await _ruleRepository.ListAsync(cancellationToken);
        var existingIds = existing.Select(r => r.RuleId).ToHashSet(StringComparer.Ordinal);

        var inserted = rules.Count(r => !existingIds.Contains(r.RuleId));
        var updated = rules.Count - inserted;

        await _ruleRepository.UpsertRangeAsync(rules, cancellationToken);

        _logger.LogInformation("Rules upserted. Inserted={Inserted}, Updated={Updated}.", inserted, updated);

        return new SeedRulesResult(loadedCount: rules.Count, insertedCount: inserted, updatedCount: updated);
    }
}