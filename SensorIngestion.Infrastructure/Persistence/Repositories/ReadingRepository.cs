using Microsoft.EntityFrameworkCore;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Domain.Entities;
using SensorIngestion.Domain.Enums;

namespace SensorIngestion.Infrastructure.Persistence.Repositories;

public class ReadingRepository : IReadingRepository
{
    private readonly SensorIngestionDbContext _dbContext;

    public ReadingRepository(SensorIngestionDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<int> InsertNewAsync(List<SensorReading> readings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(readings);

        if (readings.Count == 0)
            return 0;

        var deviceIds = readings.Select(r => r.DeviceId).Distinct().ToList();
        var metrics = readings.Select(r => r.Metric).Distinct().ToList();

        var existingKeys = await _dbContext.Readings
            .AsNoTracking()
            .Where(r => deviceIds.Contains(r.DeviceId) && metrics.Contains(r.Metric))
            .Select(r => new { r.DeviceId, r.Metric, r.Timestamp, r.Sequence })
            .ToListAsync(cancellationToken);

        var existingSet = existingKeys
            .Select(k => (k.DeviceId, k.Metric, k.Timestamp, k.Sequence))
            .ToHashSet();

        var toInsert = new List<SensorReading>();

        foreach (var reading in readings)
        {
            var key = (reading.DeviceId, reading.Metric, reading.Timestamp, reading.Sequence);
            if (existingSet.Add(key))
                toInsert.Add(reading);
        }

        if (toInsert.Count == 0)
            return 0;

        _dbContext.Readings.AddRange(toInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return toInsert.Count;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Readings.CountAsync(cancellationToken);
    }

    public async Task<List<SensorReading>> ListUnprocessedBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be positive.");

        return await _dbContext.Readings
            .Include(r => r.Violations)
            .Where(r => r.Classification == ReadingClassification.Unprocessed)
            .OrderBy(r => r.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> ListDeviceIdsForMetricAsync(
        string metric,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        return await _dbContext.Readings
            .AsNoTracking()
            .Where(r => r.Metric == metric)
            .Select(r => r.DeviceId)
            .Distinct()
            .OrderBy(id => id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SensorReading>> ListByDeviceAndMetricTrackedAsync(
        string deviceId,
        string metric,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        return await _dbContext.Readings
            .Include(r => r.Violations)
            .Where(r => r.DeviceId == deviceId && r.Metric == metric)
            .OrderBy(r => r.Timestamp)
            .ThenBy(r => r.Sequence)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void ClearTracking()
    {
        _dbContext.ChangeTracker.Clear();
    }
}