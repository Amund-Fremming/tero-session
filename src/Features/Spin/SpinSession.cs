using System.Text.Json.Serialization;
using tero.session.src.Core;

namespace tero.session.src.Features.Spin;

public class SpinSession : IJoinableSession, ICleanuppable<SpinSession>
{
    [JsonPropertyName("spin_id")]
    public Guid SpinId { get; private set; }

    [JsonPropertyName("base_id")]
    public Guid BaseId { get; private set; }

    [JsonPropertyName("host_id")]
    public Guid HostId { get; private set; }

    [JsonPropertyName("name")]
    public string Name { get; private set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; private set; }

    [JsonPropertyName("state")]
    public SpinGameState State { get; private set; }

    [JsonPropertyName("category")]
    public GameCategory Category { get; private set; }

    [JsonPropertyName("iterations")]
    public int Iterations { get; private set; }

    [JsonPropertyName("current_iteration")]
    public int CurrentIteration { get; private set; }

    [JsonPropertyName("times_played")]
    public int TimesPlayed { get; private set; }

    [JsonPropertyName("last_played")]
    public DateTime LastPlayed { get; private set; }

    [JsonPropertyName("rounds")]
    public List<string> Rounds { get; private set; } = [];

    [JsonPropertyName("players")]
    public Dictionary<Guid, int> Users { get; private set; } = [];

    [JsonConstructor]
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

    public Result<SpinSession, Error> AddUser(Guid userId)
    {
        if (State != SpinGameState.Initialized)
        {
            return Error.GameClosed;
        }

        if (Users.Count == 0 || (Users.Count == 1 && Users.ContainsKey(userId)))
        {
            HostId = userId;
            Users.Add(userId, 0);
            return this;
        }

        var exists = Users.ContainsKey(userId);
        if (exists)
        {
            return this;
        }

        Users.Add(userId, 0);
        return this;
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

    public bool IsHost(Guid userId) => HostId == userId;

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
