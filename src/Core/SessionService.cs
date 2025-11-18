using System.Text.Json;
using tero_session.src.Features.Quiz;
using tero_session.src.Features.Spin;

namespace tero_session.src.Core;

public class SessionService(SessionCache<SpinSession> spinCache, SessionCache<QuizSession> quizCache)
{
    public async Task<Result<bool, string>> InitiateGameSession(GameType gameType, string key, JsonElement value)
    {
        var result = gameType switch
        {
            GameType.Spin => await AddSessionToCache(spinCache, key, value),
            GameType.Quiz => await AddSessionToCache(quizCache, key, value),
            _ => Result<bool, string>.Err($"Unknown game type: {gameType}")
        };

        return result;
    }

    public async Task<Result<bool, string>> AddUserToGameSession(GameType gameType, string key, Guid userId)
    {
        var result = gameType switch
        {
            GameType.Spin => await AddUserToSession(spinCache, key, userId),
            _ => Result<bool, string>.Err("Session is not user joinable")
        };

        return result;
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
