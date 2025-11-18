using Newtonsoft.Json;

namespace tero_session.src.Features.Spin;

public class SpinSession : IJoinableSession
{
    [JsonProperty("spin_id")]
    public Guid SpinId { get; set; }

    [JsonProperty("base_id")]
    public Guid BaseId { get; set; }

    [JsonProperty("host_id")]
    public Guid HostId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("game_type")]
    public GameType GameType { get; set; }

    [JsonProperty("category")]
    public GameCategory Category { get; set; }

    [JsonProperty("iterations")]
    public int Iterations { get; set; }

    [JsonProperty("times_played")]
    public int TimesPlayed { get; set; }

    [JsonProperty("last_played")]
    public DateTime LastPlayed { get; set; }

    [JsonProperty("rounds")]
    public List<string> Rounds { get; set; } = [];

    [JsonProperty("players")]
    public List<SpinGamePlayer> Players { get; set; } = [];

    private SpinSession() { }

    public void AddToSession(Guid userId)
    {
        throw new NotImplementedException();
    }

    // TODO - implement core logic
}
