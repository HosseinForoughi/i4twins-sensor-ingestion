using SensorIngestion.Application.UseCases.GetAggregates;

namespace SensorIngestion.Application.Abstractions;

public interface IReadingAggregationQuery
{
    Task<List<AggregateBucketDto>> GetAggregatesAsync(GetAggregatesRequest request, CancellationToken cancellationToken = default);
}