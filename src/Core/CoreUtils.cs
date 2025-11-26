using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using tero.session.src.Features.Spin;

namespace tero.session.src.Core;

public static class CoreUtils
{
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