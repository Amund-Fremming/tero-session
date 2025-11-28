using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using tero.session.src.Features.Quiz;
using tero.session.src.Features.Spin;

namespace tero.session.src.Core;

[ApiController]
[Route("session")]
public class SessionController(
    ILogger<SessionController> logger,
    GameSessionCache<SpinSession> spinCache,
    GameSessionCache<QuizSession> quizCache
) : ControllerBase
{
    [HttpPost("initiate/{gameType}/{key}")]
    public async Task<IActionResult> InitiateGameSession(GameType gameType, string key, [FromBody] JsonElement value)
    {
        try
        {
            // TODO - remove
            logger.LogDebug($"Recieved request for {gameType} with key: {key}");

            var (statusCode, message) = gameType switch
            {
                GameType.Spin => await CoreUtils.InsertPayload(spinCache, key, value),
                GameType.Quiz=> await CoreUtils.InsertPayload(quizCache, key, value),
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