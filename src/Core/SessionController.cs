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
public class SessionController(ILogger<SessionController> logger, SessionCache<SpinSession> spinCache, SessionCache<QuizSession> quizCache) : ControllerBase
{
    [HttpPost("initiate/{gameType}/{key}")]
    public async Task<IActionResult> InitiateGameSession(GameType gameType, string key, [FromBody] GameSessionRequest request)
    {
        try
        {
            var result = gameType switch
            {
                GameType.Spin => await AddSessionToCache(spinCache, key, request.Value),
                GameType.Quiz => await AddSessionToCache(quizCache, key, request.Value),
                _ => Result<bool, string>.Err($"Unknown game type: {gameType}")
            };

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

    private static async Task<Result<bool, string>> AddSessionToCache<T>(ISessionCache<T> cache, string key, JsonElement value) where T : class
    {
        var session = JsonSerializer.Deserialize<T>(value);
        if (session is null)
        {
            return "Failed to deserialize session";
        }

        var result = await cache.Insert(key, session);
        if (result.IsErr())
        {
            return "Failed to add user to game";
        }

        return result.Unwrap();
    }

    [HttpPost("join/{gameType}/{key}/user/{userId}")]
    public async Task<IActionResult> AddUserToSession(GameType gameType, string key, Guid userId)
    {
        try
        {
            var result = gameType switch
            {
                GameType.Spin => await AddUserToSession(spinCache, key, userId),
                _ => Result<bool, string>.Err("Session is not user joinable")
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

    private static async Task<Result<bool, string>> AddUserToSession<T>(ISessionCache<T> cache, string key, Guid userId) where T : IJoinableSession
    {
        var result = await cache.Get(key);
        if (result.IsErr())
        {
            return "Failed to get session";
        }

        var session = result.Unwrap();
        session.AddToSession(userId);

        var updateResult = await cache.Update(key, session);
        if (updateResult.IsErr())
        {
            return "Failed to update session in cache";
        }

        return true;
    }

}