using Newtonsoft.Json;

namespace tero_session.src.Features.Quiz;

public class QuizSession
{
    [JsonProperty("base_id")]
    public Guid BaseId { get; set; }

    [JsonProperty("quiz_id")]
    public Guid QuizId { get; set; }

    [JsonProperty("host_id")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [JsonProperty("game_type")]
    public GameType GameType { get; set; }

    [JsonProperty("category")]
    public GameCategory Category { get; set; }

    [JsonProperty("iterations")]
    public int Iterations { get; set; }

    [JsonProperty("current_iteration")]
    public int CurrentIteration { get; set; }

    [JsonProperty("questions")]
    public List<string> Questions { get; set; } = new();

    [JsonProperty("times_played")]
    public int TimesPlayed { get; set; }

    private QuizSession() { }

    // TODO - implement core logic
}
