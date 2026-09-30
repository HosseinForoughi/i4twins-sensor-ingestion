using Microsoft.AspNetCore.Mvc;
using SensorIngestion.Api.Models;
using SensorIngestion.Application.Observability;
using SensorIngestion.Application.UseCases.ProcessPipeline;
using System.Diagnostics;

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
    private readonly ILogger<IngestController> _logger;

    public IngestController(ProcessPipelineUseCase processPipeline, ILogger<IngestController> logger)
    {
        _processPipeline = processPipeline ?? throw new ArgumentNullException(nameof(processPipeline));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    /// <response code="500">Unexpected processing failure.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProcessPipelineResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProcessPipelineResult>> Post(CancellationToken cancellationToken)
    {
        using var activity = ApplicationTelemetry.StartActivity("Ingest.Post");
        _logger.LogInformation("Ingest endpoint invoked.");

        var result = await _processPipeline.ExecuteAsync(cancellationToken);

        _logger.LogInformation(
            "Ingest endpoint completed. Inserted={Inserted}, AlertsInserted={AlertsInserted}",
            result.Ingest.NewlyInsertedReadings,
            result.SustainedAbove.AlertsInserted);

        activity?.SetStatus(ActivityStatusCode.Ok);
        return Ok(result);
    }
}