using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;
using tero_session.src.Features.Quiz;
using tero_session.src.Features.Spin;

namespace tero_session.src.Core;

[ApiController]
[Route("session")]
public class SessionController(ILogger<SessionController> logger, SessionService sessionService) : ControllerBase
{
    [HttpPost("initiate/{gameType}/{key}")]
    public async Task<IActionResult> InitiateGameSession(GameType gameType, string key, [FromBody] GameSessionRequest request)
    {
        try
        {
            var result = await sessionService.InitiateGameSession(gameType, key, request.Value);

            if (result.IsErr())
            {
                return StatusCode(500, result.Err());
            }

            var keyExists = result.Unwrap();
            if (keyExists)
            {
                return Conflict("Key already exists");
            }

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
            var result = await sessionService.AddUserToGameSession(gameType, key, userId);

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