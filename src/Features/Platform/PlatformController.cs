using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.ObjectPool;
using tero.session.src.Core;
using tero.session.src.Features.Quiz;
using tero.session.src.Features.Spin;

namespace tero.session.src.Features.Platform;

[ApiController]
[Route("session")]
public class PlatformController(
    ILogger<PlatformController> logger,
    PlatformClient platformClient,
    GameSessionCache<SpinSession> spinCache,
    GameSessionCache<QuizSession> quizCache,
    HubConnectionManager<SpinSession> spinManager,
    HubConnectionManager<QuizSession> quizManager
) : ControllerBase
{
    [HttpPost("initiate/{gameType}")]
    public IActionResult InitiateGameSession(GameType gameType, [FromBody] InitiateGameRequest request)
    {
        try
        {
            var key = request.Key;
            logger.LogInformation("Recieved request for {GameType} with key: {string}", gameType, key);
            var (statusCode, response) = gameType switch
            {
                GameType.Spin => CoreUtils.InsertPayload(platformClient, spinCache, key, request.Value),
                GameType.Quiz => CoreUtils.InsertPayload(platformClient, quizCache, key, request.Value),
                _ => (400, "Not supported game type")
            };

            return StatusCode(statusCode, response);
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Warning)
                .WithFunctionName("InitiateGameSession")
                .WithDescription("PlatformController catched a error")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(InitiateGameSession));
            return StatusCode(500, "Internal server error");
        }
    }

    // TODO - add this endpoint to admin dashboard
    [HttpGet]
    public IActionResult CacheInfo()
    {
        try
        {
            var payload = new CacheInfo
            {
                SpinSessionSize = spinCache.Size(),
                SpinManagerSize = spinManager.Size(),
                QuizSessionSize = quizCache.Size(),
                QuizManagerSize = quizManager.Size()
            };

            return Ok(payload);
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Warning)
                .WithFunctionName("CacheInfo")
                .WithDescription("CacheInfo catched a error")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(CacheInfo));
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("count/{gameType}/{gameKey}")]
    public async Task<IActionResult> NumberOfPlayers(GameType gameType, string gameKey)
    {
        try
        {
            int num = 0;
            switch (gameType)
            {
                case GameType.Spin:
                    var spinResult = await spinCache.Get(gameKey);
                    if (spinResult.IsErr())
                    {
                        // TODO - log?
                        return StatusCode(500, "Failed to get game entry from cache");
                    }
                    var spinSession = spinResult.Unwrap();
                    num = spinSession.UsersCount();
                    break;
                default:
                    return StatusCode(500, "Game type not supported");
            }

            return Ok(num);
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Warning)
                .WithFunctionName("GetCurrentPlayers")
                .WithDescription("Threw an exception")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(CacheInfo));
            return StatusCode(500, "Internal server error");
        }
    }
}