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
    public SpinGameState State{ get; private set; }

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

    public void AddUser(Guid userId)
    {
        var user = SpinGamePlayer.Create(userId);
        Players.Add(user);
    }

    public IEnumerable<Guid> SelectRoundPlayers()
    {
        // TODO
        return null;
    }

    public void IncrementPlayersChosen(IEnumerable<Guid> chosen)
    {
        // TODO
    }

    public void NextRound()
    {
        // Do state check so this cannot be ran before
        // Return state
        // TODO
    }

    public void AddRound(string round)
    {
        Rounds.Add(round); 
        Iterations++;
    } 

    public int IterationsCount() => Iterations;
    public int PlayersCount() => Players.Count;

    public SpinSession Start()
    {
        Players.Shuffle();
        Rounds.Shuffle();
        return this;
    }

    // TODO - implement core logic
}
