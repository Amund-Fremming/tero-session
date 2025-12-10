using System.Text.Json.Serialization;
using tero.session.src.Core;

namespace tero.session.src.Features.Quiz;

public class QuizSession
{
    [JsonPropertyName("base_id")]
    public Guid BaseId { get; private set; }

    [JsonPropertyName("quiz_id")]
    public Guid QuizId { get; private set; }

    [JsonPropertyName("host_id")]
    public Guid HostId { get; private set; }

    [JsonPropertyName("name")]
    public string Name { get; private set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; private set; }

    [JsonPropertyName("category")]
    public GameCategory Category { get; private set; }

    [JsonPropertyName("iterations")]
    public int Iterations { get; private set; }

    [JsonPropertyName("current_iteration")]
    public int CurrentIteration { get; private set; }

    [JsonPropertyName("questions")]
    public List<string> Questions { get; private set; } = new();

    [JsonPropertyName("times_played")]
    public int TimesPlayed { get; private set; }

    [JsonConstructor]
    private QuizSession() { }

    public QuizSession AddQuesiton(string question)
    {
        Questions.Add(question);
        Iterations++;
        return this;
    }

    public QuizSession Start()
    {
        Questions.Shuffle();
        return this;
    }
}