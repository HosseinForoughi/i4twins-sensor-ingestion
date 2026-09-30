using Microsoft.AspNetCore.Mvc;
using SensorIngestion.Application.UseCases.GetAggregates;

namespace SensorIngestion.Api.Controllers;

[ApiController]
[Route("api/aggregates")]
public class AggregatesController : ControllerBase
{
    private readonly GetAggregatesUseCase _getAggregates;

    public AggregatesController(GetAggregatesUseCase getAggregates)
    {
        _getAggregates = getAggregates ?? throw new ArgumentNullException(nameof(getAggregates));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AggregateBucketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<AggregateBucketDto>>> Get([FromQuery] string deviceId,
        [FromQuery] string metric,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] int bucketSeconds,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetAggregatesRequest(deviceId, metric, from, to, bucketSeconds);
            var result = await _getAggregates.ExecuteAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}