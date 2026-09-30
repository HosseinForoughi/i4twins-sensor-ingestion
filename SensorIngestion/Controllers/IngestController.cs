using Microsoft.AspNetCore.Mvc;
using SensorIngestion.Application.UseCases.ProcessPipeline;

namespace SensorIngestion.Api.Controllers;

/// <summary>
/// Runs the full ingest and rule-evaluation pipeline against the configured readings file.
/// </summary>
[ApiController]
[Route("api/ingest")]
[Produces("application/json")]
[Tags("Ingest")]
public class IngestController : ControllerBase
{
    private readonly ProcessPipelineUseCase _processPipeline;

    public IngestController(ProcessPipelineUseCase processPipeline)
    {
        _processPipeline = processPipeline ?? throw new ArgumentNullException(nameof(processPipeline));
    }

    /// <summary>
    /// Process readings from the configured JSONL file.
    /// </summary>
    /// <remarks>
    /// Pipeline steps:
    /// 1. Load, validate, and first-wins dedupe readings in batches
    /// 2. Persist new readings (idempotent on natural key)
    /// 3. Evaluate instantaneous rules and classify readings
    /// 4. Detect SustainedAbove episodes, apply cooldown, and store alerts
    ///
    /// Re-running the same file does not create duplicate readings or alerts.
    /// </remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Pipeline completed; returns processing counts for ingest, instantaneous rules, and SustainedAbove.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProcessPipelineResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcessPipelineResult>> Post(CancellationToken cancellationToken)
    {
        var result = await _processPipeline.ExecuteAsync(cancellationToken);
        return Ok(result);
    }
}