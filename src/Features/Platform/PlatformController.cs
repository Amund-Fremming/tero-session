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
    GameSessionCache<SpinSession> spinCache,
    GameSessionCache<QuizSession> quizCache,
    HubConnectionManager<SpinSession> spinManager,
    HubConnectionManager<QuizSession> quizManager
) : ControllerBase
{
    [HttpPost("initiate/{gameType}/{key}")]
    public IActionResult InitiateGameSession(GameType gameType, string key, [FromBody] JsonElement value)
    {
        try
        {
            logger.LogDebug("Recieved request for {GameType} with key: {string}", gameType, key);
            var (statusCode, message) = gameType switch
            {
                GameType.Spin => CoreUtils.InsertPayload(spinCache, key, value),
                GameType.Quiz => CoreUtils.InsertPayload(quizCache, key, value),
                _ => (400, "Game type not supported")
            };

            return StatusCode(statusCode, message);
        }
        catch (Exception error)
        {
            // TODO - system log 
            logger.LogError(error, nameof(InitiateGameSession));
            return StatusCode(500, "Internal server error II");
        }
    }

    public async Task<IActionResult> CacheInfo()
    {
        try
        {
            await Task.Run(() => { });
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
            // TODO - system log 
            logger.LogError(error, nameof(CacheInfo));
            return StatusCode(500, "Internal server error II");
        }
    }
}