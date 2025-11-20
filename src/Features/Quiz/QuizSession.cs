using Newtonsoft.Json;
using tero.session.src.Core;

namespace tero.session.src.Features.Quiz;

public class QuizSession
{
    [JsonProperty("base_id")]
    public Guid BaseId { get; private set; }

    [JsonProperty("quiz_id")]
    public Guid QuizId { get; private set; }

    [JsonProperty("host_id")]
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    [JsonProperty("state")]
    public QuizGameState State { get; private set; }

    [JsonProperty("category")]
    public GameCategory Category { get; private set; }

    [JsonProperty("iterations")]
    public int Iterations { get; private set; }

    [JsonProperty("current_iteration")]
    public int CurrentIteration { get; private set; }

    [JsonProperty("questions")]
    public List<string> Questions { get; private set; } = new();

    [JsonProperty("times_played")]
    public int TimesPlayed { get; private set; }

    private QuizSession() { }

    public void AddQuesiton(string question)
    {
        Questions.Add(question);
        Iterations++;
    }

    public QuizSession Start()
    {
        Questions.Shuffle();
        return this;
    }
}