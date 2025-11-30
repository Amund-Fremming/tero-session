using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace tero.session.src.Core;

public static class CoreUtils
{
    public static (int, string) InsertPayload<TSession>(GameSessionCache<TSession> cache, string key, JsonElement value)
    {
        try
        {
        var spinSession = JsonSerializer.Deserialize<TSession>(value);
        if (spinSession is null)
        {
            return (400, "Invalid payload");
        }

        var spinResult = cache.Insert(key, spinSession);
        if (spinResult.IsErr())
        {
            return spinResult.Err() switch
            {
                Error.KeyExists => (409, "Game key in use"),
                _ => (500, "Internal server error")
            };
        }

        return (200, "Game initialized");
        }
        catch (JsonException)
        {
            return (500, "JSON error");
        }
        catch (Exception)
        {
            return (500, "Internal server error");
        }
    }
    


    public static async Task Broadcast(IHubCallerClients clients, Error error)
    {
        switch (error)
        {
            case Error.GameNotFound:
                await clients.Caller.SendAsync("error", "Spillet finnes ikke");
                break;
            case Error.GameClosed:
                await clients.Caller.SendAsync("error", "Spillet er lukket for fler handlinger");
                break;
            case Error.GameFinished:
                await clients.Caller.SendAsync("error", "Spillet er ferdig");
                break;
            case Error.System:
                await clients.Caller.SendAsync("error", "En feil har skjedd, forsøk igjen senere");
                break;
            default:
                await clients.Caller.SendAsync("error", "En feil har skjedd, forsøk igjen senere");
                break;
        }
    }
}