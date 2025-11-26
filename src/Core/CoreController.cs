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

            switch (gameType)
            {
                case GameType.Spin:
                    var session = JsonSerializer.Deserialize<SpinSession>(request.Value);
                    await cache.Insert<SpinSession>(key, session);
                    break;
                case GameType.Quiz:
                    var session = JsonSerializer.Deserialize<QuizSession>(request.Value);
                    await cache.Insert<QuizSession>(key, session);
                    break;
            }

            /* return StatusCode(500, result.Err());

            var keyExists = result.Unwrap();
            if (keyExists)
            {
                return Conflict("Key already exists");
            }
            */

            return Ok("Game initialized");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("join/{gameType}/{key}/user/{userId}")]
    public async Task<IActionResult> AddUserToSession(GameType gameType, string key, Guid userId)
    {
        try
        {
            var result = gameType switch
            {
                GameType.Spin => await cache.AddUserToSession<SpinSession>(key, userId),
                _ => new InvalidOperationException("Game type not supported")
            };

            if (result.IsErr())
            {
                return StatusCode(500, result.Err());
            }

            return Ok("User added to session");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error");
            return StatusCode(500, "Internal server error");
        }
    }
}