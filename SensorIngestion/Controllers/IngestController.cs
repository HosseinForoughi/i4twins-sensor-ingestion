using Microsoft.AspNetCore.Mvc;
using SensorIngestion.Application.UseCases.ProcessPipeline;

namespace SensorIngestion.Api.Controllers;

[ApiController]
[Route("api/ingest")]
public class IngestController : ControllerBase
{
    private readonly ProcessPipelineUseCase _processPipeline;

    public IngestController(ProcessPipelineUseCase processPipeline)
    {
        _processPipeline = processPipeline ?? throw new ArgumentNullException(nameof(processPipeline));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProcessPipelineResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcessPipelineResult>> Post(CancellationToken cancellationToken)
    {
        var result = await _processPipeline.ExecuteAsync(cancellationToken);
        return Ok(result);
    }
}