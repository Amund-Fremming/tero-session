using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace tero_session.Common;

[ApiController]
[Route("session")]
public class GameSessionController(ILogger<GameSessionController> logger) : ControllerBase
{
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiateSession([FromBody] GameSessionRequest request)
    {
        logger.LogInformation($"Initiating {request.GameType} session");
        // TODO
        return Ok();
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateSession([FromBody] GameSessionRequest request)
    {
        logger.LogInformation($"Creating {request.GameType} session");
        // TODO
        return Ok();
    }
}