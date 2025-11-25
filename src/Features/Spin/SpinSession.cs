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

    public void RemoveUser(Guid userId)
    {
        Users.Remove(userId);
    }

    public bool AddUser(Guid userId)
    {
        var exists = Users.ContainsKey(userId);
        if (exists)
        {
            return false;
        }

        Users.Add(userId, 0);
        return true;
    }

    public IEnumerable<Guid> GetSpinResult()
    {
        if (Users.Count == 0)
        {
            return [];
        }

        var rnd = new Random();
        var r = rnd.NextDouble();

        var i = 0;
        var selected = new List<Guid>(Users.Count / 2);
        while (selected.Count < Users.Count)
        {
            if (i == Users.Count)
            {
                r = rnd.NextDouble();
                i = 0;
                continue;
            }

            var (userId, timesChosen)= Users.ElementAt(i);
            var playerWeight = 1 - timesChosen / Iterations;
            if (playerWeight > r)
            {
                selected.Add(userId);
                Users[userId] = timesChosen++;
            }
            i++;
        }

        return selected.ToList().Shuffle();
    }

    /// Returns the round challenge
    public string NextRound()
    {
        // Add check for if its more rounds or not
        // Do state check so this cannot be ran before
        // Return state
        // TODO
        return "";
    }

    public bool StartSpin()
    {
        return false;
    }

    public bool AddRound(string round)
    {
        if (State == SpinGameState.Closed)
        {
            return false;
        }

        Rounds.Add(round);
        Iterations++;
        return true;
    }

    public int IterationsCount() => Iterations;
    public int PlayersCount() => Users.Count;

    public SpinSession Start()
    {
        CurrentIteration = 0;
        State = SpinGameState.Closed;
        Rounds.Shuffle();
        return this;
    }

    // TODO - implement core logic
    /*
        Add current iteraion
        add host
        add Update host / remove old
        
    */
}
