using Microsoft.Extensions.Logging;
using SensorIngestion.Application.Abstractions;
using SensorIngestion.Application.Observability;

namespace SensorIngestion.Application.UseCases.GetAggregates;

public class GetAggregatesUseCase
{
    private readonly IReadingAggregationQuery _aggregationQuery;
    private readonly ILogger<GetAggregatesUseCase> _logger;

    public GetAggregatesUseCase(IReadingAggregationQuery aggregationQuery, ILogger<GetAggregatesUseCase> logger)
    {
        _aggregationQuery = aggregationQuery ?? throw new ArgumentNullException(nameof(aggregationQuery));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<AggregateBucketDto>> ExecuteAsync(GetAggregatesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var activity = ApplicationTelemetry.StartActivity("Aggregates.Query");
        activity?.SetTag("device.id", request.DeviceId);
        activity?.SetTag("metric", request.Metric);

        try
        {
            var result = await _aggregationQuery.GetAggregatesAsync(request, cancellationToken);
            _logger.LogDebug(
                "Aggregate query returned {BucketCount} buckets for {DeviceId}/{Metric}.",
                result.Count,
                request.DeviceId,
                request.Metric);
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, ex.Message);
            _logger.LogError(
                ex,
                "Aggregate query failed for {DeviceId}/{Metric}.",
                request.DeviceId,
                request.Metric);
            throw;
        }
    }
}