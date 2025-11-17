using System.ComponentModel.DataAnnotations;
using tero_session.src.Core;
using tero_session.src.Features.Spin;

namespace tero_session.src.Features.Quiz;

// Main AskGame class (renamed to match Quiz feature)
public sealed class AskGame : GameBase
{
    public int CreatorId { get; set; }
    public Category Category { get; private set; }
    public AskGameState State { get; private set; }
    
    [MaxLength(100)]
    public string? Description { get; set; } 

    private readonly List<Question> _questions = [];
    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

    private AskGame()
    { }

    public Result<int> AddQuestion(Question question)
    {
        if (question is null)
        {
            return new Error("Question cannot be null.");
        }

        _questions.Add(question);
        Iterations++;
        return Iterations;
    }

    public Result<AskGame> StartGame()
    {
        State = AskGameState.Closed;
        _questions.Shuffle();
        return this;
    }

    public static AskGame Create(int userId, string name, string description = "", Category category = Category.Random)
    {
        var game = new AskGame()
        {
            IsCopy = false,
            CreatorId = userId,
            Category = category,
            State = AskGameState.Initialized,
            Name = name,
            Iterations = 0,
            CurrentIteration = 0,
            Description = description,
        };

        game.UniversalId = int.Parse("1" + game.Id);
        return game;
    }

    public override void SetUniversalId() => UniversalId = int.Parse("1" + Id);
}

// List extension for shuffling
public static class ListExtensions
{
    public static List<T> Shuffle<T>(this List<T> list)
    {
        var random = new Random();

        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }
}
