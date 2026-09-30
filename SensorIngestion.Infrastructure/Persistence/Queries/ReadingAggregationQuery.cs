using Microsoft.EntityFrameworkCore;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.UseCases.GetAggregates;
using SensorIngestion.Domain.Enums;

namespace SensorIngestion.Infrastructure.Persistence.Queries;

public class ReadingAggregationQuery : IReadingAggregationQuery
{
    private readonly SensorIngestionDbContext _dbContext;

    public ReadingAggregationQuery(SensorIngestionDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<List<AggregateBucketDto>> GetAggregatesAsync(GetAggregatesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var readings = await _dbContext.Readings.AsNoTracking()
            .Where(r =>
                r.DeviceId == request.DeviceId &&
                r.Metric == request.Metric &&
                r.Classification == ReadingClassification.Acceptable)
            .Select(r => new { r.Timestamp, r.Value })
            .ToListAsync(cancellationToken);

        var inRange = readings
            .Where(r => r.Timestamp >= request.From && r.Timestamp < request.To)
            .ToList();

        if (inRange.Count == 0)
            return [];

        var bucketSeconds = request.BucketSeconds;
        var from = request.From;

        return inRange
            .GroupBy(r =>
            {
                var offsetSeconds = (r.Timestamp - from).TotalSeconds;
                var bucketIndex = (long)Math.Floor(offsetSeconds / bucketSeconds);
                return from.AddSeconds(bucketIndex * bucketSeconds);
            })
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var values = g.Select(x => x.Value).ToList();
                return new AggregateBucketDto(
                    bucketStart: g.Key,
                    count: values.Count,
                    average: values.Average(),
                    min: values.Min(),
                    max: values.Max());
            })
            .ToList();
    }
}