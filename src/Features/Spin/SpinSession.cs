using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using tero.session.src.Core;
using tero.session.src.Core.Spin;

namespace tero.session.src.Features.Spin;

public class SpinSession : IJoinableSession, ICleanuppableSession<SpinSession>
{
    [JsonProperty("spin_id")]
    public Guid SpinId { get; private set; }

    [JsonProperty("base_id")]
    public Guid BaseId { get; private set; }

    [JsonProperty("host_id")]
    public Guid HostId { get; private set; }

    [JsonProperty("name")]
    public string Name { get; private set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; private set; }

    [JsonProperty("state")]
    public SpinGameState State { get; private set; }

    [JsonProperty("category")]
    public GameCategory Category { get; private set; }

    [JsonProperty("iterations")]
    public int Iterations { get; private set; }

    [JsonProperty("current_iteration")]
    public int CurrentIteration { get; private set; }

    [JsonProperty("times_played")]
    public int TimesPlayed { get; private set; }

    [JsonProperty("last_played")]
    public DateTime LastPlayed { get; private set; }

    [JsonProperty("rounds")]
    public List<string> Rounds { get; private set; } = [];

    [JsonProperty("players")]
    public Dictionary<Guid, int> Users { get; private set; } = [];

    private SpinSession() { }

    public List<Guid> GetUserIds() => Users.Select(u => u.Key).ToList().Shuffle();
    public int UsersCount() => Users.Count;

    public Option<Guid> RemoveUser(Guid userId)
    {
        if (userId == HostId)
        {
            var hostId = SetNewHost();
            return Option<Guid>.Some(hostId);
        }

        Users.Remove(userId);
        return Option<Guid>.None;
    }

    public Option<Guid> AddUser(Guid userId)
    {
        if (Users.Count == 0 || (Users.Count == 1 && Users.ContainsKey(userId)))
        {
            HostId = userId;
            Users.Add(userId, 0);
            return Option<Guid>.Some(userId);
        }

        var exists = Users.ContainsKey(userId);
        if (exists)
        {
            return Option<Guid>.None;
        }

        Users.Add(userId, 0);
        return Option<Guid>.None;
    }

    public HashSet<Guid> GetSpinResult(int numPlayers)
    {
        if (Users.Count == 0)
        {
            return [];
        }

        var rnd = new Random();
        var r = rnd.NextDouble();

        int i = 0;
        var selected = new HashSet<Guid>(numPlayers);
        while (selected.Count < numPlayers)
        {
            if (i == Users.Count)
            {
                r = rnd.NextDouble();
                i = 0;
                continue;
            }

            var (userId, timesChosen) = Users.ElementAt(i);
            var playerWeight = 1 - timesChosen / Iterations;
            if (playerWeight > r)
            {
                selected.Add(userId);
                Users[userId] = timesChosen++;
            }
            i++;
        }

        return selected;
    }

    public Result<SpinSession, Error> IncrementRound()
    {
        if (CurrentIteration == Iterations)
        {
            State = SpinGameState.Finished;
            return Error.GameFinished;
        }

        State = SpinGameState.RoundInitialized;
        CurrentIteration++;
        return this;
    }

    public string GetRoundText() => Rounds.ElementAt(CurrentIteration);

    public Result<SpinSession, Error> AddRound(string round)
    {
        if (State != SpinGameState.Initialized)
        {
            return Error.GameClosed;
        }

        Rounds.Add(round);
        Iterations++;
        return this;
    }


    public SpinSession Start()
    {
        CurrentIteration = 0;
        State = SpinGameState.RoundInitialized;
        Rounds.Shuffle();
        return this;
    }

    private Guid SetNewHost()
    {
        var (userId, _) = Users.ElementAt(0);
        HostId = userId;
        return userId;
    }

    public SpinSession Cleanup(Guid userId)
    {
        Users.Remove(userId);
        return this;
    }
}
