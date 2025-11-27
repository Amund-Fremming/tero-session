using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using tero.session.src.Features.Quiz;
using tero.session.src.Features.Spin;

namespace tero.session.src.Core;

[ApiController]
[Route("session")]
public class SessionController(ILogger<SessionController> logger, GameSessionCache cache) : ControllerBase
{
    [HttpPost("initiate/{gameType}/{key}")]
    public async Task<IActionResult> InitiateGameSession(GameType gameType, string key, [FromBody] GameSessionRequest request)
    {
        try
        {
            var (statusCode, message) = gameType switch
            {
                GameType.Spin => await CoreUtils.InsertPayload<SpinSession>(cache, key, request.Value),
                GameType.Quiz=> await CoreUtils.InsertPayload<QuizSession>(cache, key, request.Value),
                _ => (400, "Game type not supported")
            };

            return StatusCode(statusCode, message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error");
            return StatusCode(500, "Internal server error");
        }
    }
}