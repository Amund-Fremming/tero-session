using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using tero.session.src.Core;
using tero.session.src.Core.Spin;

namespace tero.session.src.Features.Spin;

public class SpinSession : IJoinableSession
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

    /// Returns a Option<Guid> if a new host is set
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

    /// Returns a Option<Guid> if the user added becomes the host
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

    /// Returns the users chosen this round
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

    /// Returns a Ok<string> with the new round
    /// Returns a Err<SpinGameState> if the game is finished
    public Result<string, SpinGameState> NextRound()
    {
        if (CurrentIteration == Iterations)
        {
            State = SpinGameState.Finished;
            return SpinGameState.Finished;
        }

        var next = Rounds.ElementAt(CurrentIteration);
        State = SpinGameState.RoundInitialized;
        CurrentIteration++;
        return next;
    }

    public bool AddRound(string round)
    {
        if (State != SpinGameState.Initialized)
        {
            return false;
        }

        Rounds.Add(round);
        Iterations++;
        return true;
    }


    public string Start()
    {
        CurrentIteration = 0;
        State = SpinGameState.RoundInitialized;
        Rounds.Shuffle();

        var next = Rounds.ElementAt(0);
        CurrentIteration++;
        return next;
    }

    private Guid SetNewHost()
    {
        var (userId, _) = Users.ElementAt(0);
        HostId = userId;
        return userId;
    }
}
