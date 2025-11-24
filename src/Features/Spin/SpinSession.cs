using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using tero.session.src.Core;

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

    [JsonProperty("times_played")]
    public int TimesPlayed { get; private set; }

    [JsonProperty("last_played")]
    public DateTime LastPlayed { get; private set; }

    [JsonProperty("rounds")]
    public List<string> Rounds { get; private set; } = [];

    [JsonProperty("players")]
    public List<SpinGamePlayer> Players { get; private set; } = [];

    private SpinSession() { }

    public void RemoveUser(Guid userId)
    {
        Players = [.. Players.Where(p => p.UserId != userId)];
    }

    public bool AddUser(Guid userId)
    {
        var exists = Players.Any(p => p.UserId == userId);
        if (exists)
        {
            return false;
        }

        var user = SpinGamePlayer.Create(userId);
        Players.Add(user);
        return true;
    }

    public IEnumerable<Guid> SelectRoundPlayers()
    {
        if (Players.Count == 0)
        {
            return [];
        }

        var rnd = new Random();
        var playersMap = Players.ToDictionary(p => p.UserId, p => p);
        var r = rnd.NextDouble();

        var i = 0;
        var selected = new List<SpinGamePlayer>();
        while (selected.Count < Players.Count)
        {
            if (i == Players.Count)
            {
                r = rnd.NextDouble();
                i = 0;
                continue;
            }

            var player = Players[i];
            var playerWeight = 1 - player.TimesChosen / Iterations;
            if (playerWeight > r)
            {
                selected.Add(player);
            }
            i++;
        }

        foreach (var player in selected)
        {
            player.IncTimesChosen();
        }

        return selected.Select(p => p.UserId).AsEnumerable();
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
    public int PlayersCount() => Players.Count;

    public SpinSession Start()
    {
        State = SpinGameState.Closed;
        Players.Shuffle();
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
