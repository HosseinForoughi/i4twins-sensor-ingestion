using Microsoft.EntityFrameworkCore;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly SensorIngestionDbContext _dbContext;

    public AlertRepository(SensorIngestionDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<Alert>> ListForStreamAsync(string ruleId,
        string deviceId,
        string metric,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        var alerts = await _dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.RuleId == ruleId && a.DeviceId == deviceId && a.Metric == metric)
            .ToListAsync(cancellationToken);

        return alerts
            .OrderBy(a => a.StartTs)
            .ToList();
    }

    public async Task<int> InsertNewAsync(List<Alert> alerts, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alerts);

        if (alerts.Count == 0)
            return 0;

        var ruleIds = alerts.Select(a => a.RuleId).Distinct().ToList();

        var existingKeys = await _dbContext.Alerts
            .AsNoTracking()
            .Where(a => ruleIds.Contains(a.RuleId))
            .Select(a => new { a.RuleId, a.DeviceId, a.Metric, a.StartTs })
            .ToListAsync(cancellationToken);

        var existingSet = existingKeys
            .Select(k => (k.RuleId, k.DeviceId, k.Metric, k.StartTs))
            .ToHashSet();

        var toInsert = new List<Alert>();

        foreach (var alert in alerts)
        {
            var key = (alert.RuleId, alert.DeviceId, alert.Metric, alert.StartTs);
            if (existingSet.Add(key))
                toInsert.Add(alert);
        }

        if (toInsert.Count == 0)
            return 0;

        _dbContext.Alerts.AddRange(toInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return toInsert.Count;
    }
}