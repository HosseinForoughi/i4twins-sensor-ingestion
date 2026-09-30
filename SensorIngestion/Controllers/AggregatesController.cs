using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SensorIngestion.Api.Models;
using SensorIngestion.Application.Observability;
using SensorIngestion.Application.UseCases.GetAggregates;

namespace SensorIngestion.Api.Controllers;

/// <summary>
/// Time-bucket aggregates of acceptable sensor readings.
/// </summary>
[ApiController]
[Route("api/aggregates")]
[Produces("application/json")]
[Tags("Aggregates")]
public class AggregatesController : ControllerBase
{
    private readonly GetAggregatesUseCase _getAggregates;
    private readonly ILogger<AggregatesController> _logger;

    public AggregatesController(GetAggregatesUseCase getAggregates, ILogger<AggregatesController> logger)
    {
        _getAggregates = getAggregates ?? throw new ArgumentNullException(nameof(getAggregates));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get per-bucket aggregates for a device and metric.
    /// </summary>
    /// <remarks>
    /// Only readings classified as <c>Acceptable</c> are included.
    /// Buckets are half-open intervals of <c>bucketSeconds</c> over <c>[from, to)</c>.
    /// Empty buckets are omitted from the response.
    /// </remarks>
    /// <param name="query">Device, metric, time window, and bucket size.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">List of non-empty aggregate buckets ordered by bucket start.</response>
    /// <response code="400">Invalid query parameters (missing values, empty range, or non-positive bucket size).</response>
    /// <response code="500">Unexpected processing failure.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AggregateBucketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AggregateBucketDto>>> Get(
        [FromQuery] GetAggregatesQuery query,
        CancellationToken cancellationToken)
    {
        using var activity = ApplicationTelemetry.StartActivity("Aggregates.Get");
        activity?.SetTag("device.id", query.DeviceId);
        activity?.SetTag("metric", query.Metric);
        activity?.SetTag("bucket.seconds", query.BucketSeconds);

        _logger.LogInformation(
            "Aggregates requested. DeviceId={DeviceId}, Metric={Metric}, From={From}, To={To}, BucketSeconds={BucketSeconds}",
            query.DeviceId,
            query.Metric,
            query.From,
            query.To,
            query.BucketSeconds);

        var request = new GetAggregatesRequest(
            deviceId: query.DeviceId,
            metric: query.Metric,
            from: query.From,
            to: query.To,
            bucketSeconds: query.BucketSeconds);

        var result = await _getAggregates.ExecuteAsync(request, cancellationToken);

        _logger.LogInformation(
            "Aggregates completed. DeviceId={DeviceId}, Metric={Metric}, BucketCount={BucketCount}",
            query.DeviceId,
            query.Metric,
            result.Count);

        activity?.SetStatus(ActivityStatusCode.Ok);
        return Ok(result);
    }
}