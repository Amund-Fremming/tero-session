using System.Text.Json.Serialization;
using tero.session.src.Core;

namespace tero.session.src.Features.Quiz;

public class QuizSession
{
    [JsonPropertyName("base_id")]
    public Guid BaseId { get; init; }

    [JsonPropertyName("quiz_id")]
    public Guid QuizId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("category")]
    public GameCategory Category { get; init; }

    [JsonPropertyName("iterations")]
    public int Iterations { get; set; }

    [JsonPropertyName("current_iteration")]
    public int CurrentIteration { get; init; }

    [JsonPropertyName("questions")]
    public List<string> Questions { get; init; } = new();

    [JsonPropertyName("times_played")]
    public int TimesPlayed { get; init; }

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