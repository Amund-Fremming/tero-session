using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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
            var (statusCode, message) = gameType switch
            {
                GameType.Spin => CoreUtils.InsertPayload(spinCache, key, request.Value),
                GameType.Quiz => CoreUtils.InsertPayload(quizCache, key, request.Value),
                _ => (400, "Game type not supported")
            };

            return StatusCode(statusCode, message);
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

    public IActionResult CacheInfo()
    {
        try
        {
            spinManager.GetType();
            quizManager.GetType();
            /*
            var payload = new CacheInfo
            {
                SpinSessionSize = spinCache.Size(),
                SpinManagerSize = spinManager.Size(),
                QuizSessionSize = quizCache.Size(),
                QuizManagerSize = quizManager.Size()
            };

            return Ok(payload);
*/
            return Ok();
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
}