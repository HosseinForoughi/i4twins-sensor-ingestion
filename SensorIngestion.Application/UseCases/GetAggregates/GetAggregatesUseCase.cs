using SensorIngestion.Application.Abstractions;

namespace SensorIngestion.Application.UseCases.GetAggregates;

public class GetAggregatesUseCase
{
    private readonly IReadingAggregationQuery _aggregationQuery;

    public GetAggregatesUseCase(IReadingAggregationQuery aggregationQuery)
    {
        _aggregationQuery = aggregationQuery ?? throw new ArgumentNullException(nameof(aggregationQuery));
    }

    public Task<List<AggregateBucketDto>> ExecuteAsync(GetAggregatesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _aggregationQuery.GetAggregatesAsync(request, cancellationToken);
    }
}