using Microsoft.EntityFrameworkCore;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Repositories;

public class RuleRepository : IRuleRepository
{
    private readonly SensorIngestionDbContext _dbContext;

    public RuleRepository(SensorIngestionDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task UpsertRangeAsync(List<Rule> rules, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull((object)rules);

        if (rules.Count == 0) return;

        var seedIds = rules.Select(r => r.RuleId).ToList();

        var existingByRuleId = await _dbContext.Rules
            .Where(r => seedIds.Contains(r.RuleId))
            .ToDictionaryAsync(r => r.RuleId, StringComparer.Ordinal, cancellationToken);

        foreach (var seed in rules)
        {
            if (existingByRuleId.TryGetValue(seed.RuleId, out var existing))
            {
                existing.SyncFromSeed(
                    seed.Name,
                    seed.Enabled,
                    seed.Metric,
                    seed.Operator,
                    seed.DeviceId,
                    seed.Threshold,
                    seed.MinValue,
                    seed.MaxValue,
                    seed.DurationSeconds);
            }
            else
            {
                _dbContext.Rules.Add(seed);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Rule>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rules
            .AsNoTracking()
            .OrderBy(r => r.RuleId)
            .ToListAsync(cancellationToken);
    }
}