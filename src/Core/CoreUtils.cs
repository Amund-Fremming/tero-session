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

    public static async Task Broadcast(IHubCallerClients clients, Error error, ILogger logger)
    {
        // TODO - add logging here not everywhere else
        switch (error)
        {
            case Error.KeyExists:
                logger.LogError("Key already exists");
                await clients.Caller.SendAsync("error", "Spill nøkkelen er allerede i bruk");
                break;
            case Error.NotGameHost:
                logger.LogError("Non host user tried doing host operations on game");
                await clients.Caller.SendAsync("error", "Denne handlingen kan bare en host gjøre");
                break;
            case Error.GameClosed:
                logger.LogCritical("User tried doing operation on a closed game");
                await clients.Caller.SendAsync("error", "Spillet er lukket for fler handlinger");
                break;
            case Error.GameFinished:
                logger.LogCritical("User tried doing operation on a finished game");
                await clients.Caller.SendAsync("error", "Spillet er ferdig");
                break;
            case Error.GameNotFound:
                logger.LogCritical("User tried requesting non existing game");
                await clients.Caller.SendAsync("error", "Spillet finnes ikke");
                break;
            case Error.System:
                logger.LogError("System internal error");
                await clients.Caller.SendAsync("error", "En feil har skjedd, forsøk igjen senere");
                break;
            case Error.Json:
                logger.LogError("JSON error, failed serialization or object mismatch on deserialization");
                await clients.Caller.SendAsync("error", "En feil har skjedd, forsøk på nytt");
                break;
            case Error.NullReference:
                logger.LogError("Function recieved a null value");
                await clients.Caller.SendAsync("error", "Mottok en tom verdi, forsøk på nytt");
                break;
            case Error.Overflow:
                logger.LogError("Some cache was overflowed eating up all memory.");
                await clients.Caller.SendAsync("error", "En feil har skjedd, forsøk på nytt");
                break;
            case Error.Http:
                logger.LogError("Failed to create or use http client");
                await clients.Caller.SendAsync("error", "En feil har skjedd, forsøk på nytt");
                break;
            case Error.Upstream:
                logger.LogError("Failed to contact upstream service, eiterh platform or auth0");
                await clients.Caller.SendAsync("error", "En feil har skjedd, forsøk på nytt");
                break;
        }
    }
}