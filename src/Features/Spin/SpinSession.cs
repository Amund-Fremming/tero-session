using System.ComponentModel.DataAnnotations;
using tero_session.src.Core;

namespace tero_session.src.Features.Spin;

// Base abstractions
public interface IUser
{
    public int Id { get; }
    public Guid Guid { get; }
    public DateTime LastActive { get; }
}

public abstract class UserBase : IUser
{
    [Key]
    public int Id { get; set; }

    public Guid Guid { get; set; }

    public DateTime LastActive { get; set; }

    public void UpdateActivity() => LastActive = DateTime.UtcNow;
}

public abstract class GameBase
{
    [Key]
    public int Id { get; set; }

    public int UniversalId { get; set; }
    
    [MaxLength(40)]
    public string Name { get; set; } = null!;
    public int Iterations { get; set; }
    public int CurrentIteration { get; set; }
    public bool IsCopy { get; set;}

    public abstract void SetUniversalId();
}

// Main SpinGame class
public sealed class SpinGame : GameBase
{
    public Category Category { get; private set; }
    public SpinGameState State { get; private set; }
    public int HostId { get; private set; }
    public UserBase Host { get; private set; } = null!;

    private readonly IList<SpinPlayer> _players = [];
    private IList<Challenge> _challenges = [];

    public IReadOnlyList<SpinPlayer> Players => _players.AsReadOnly();
    public IReadOnlyList<Challenge> Challenges => _challenges.AsReadOnly();

    private SpinGame()
    { }
    
    public void CloseChallenges() => State = SpinGameState.ChallengesClosed;

    // TODO - remove
    public Result AddPlayer(SpinPlayer player)
    {
        if (_players.Contains(player))
        {
            return new Error("Spilleren er allerede med i spillet");
        }

        _players.Add(player);
        return Result.Ok;
    }

    public Result<SpinPlayer> UpdateHost()
    {
        var prevHost = _players.FirstOrDefault(p => p.Id == HostId);
        prevHost?.SetActive(false);

        if (_players.Count == 0)
        {
            return new Error("The game does not have any players left.");
        }

        var nextHost = _players[0];
        HostId = nextHost.Id;
        return nextHost;
    }

    public Result<int> AddChallenge(Challenge challenge)
    {
        if (challenge is null)
        {
            return new Error("Challenge cannot be null.");
        }

        if (State != SpinGameState.Initialized)
        {
            return new Error("Cannot add challenges in a started game.");
        }

        _challenges.Add(challenge);
        Iterations++;
        return Iterations;
    }

    private List<SpinPlayer> SelectPlayers(int numberOfPlayers)
    {
        if (_players.Count == 0)
        {
            return [];
        }

        var rnd = new Random();
        var playersMap = _players.ToDictionary(p => p.Id, p => p);
        var r = rnd.NextDouble();
        
        var i = 0;
        var selected = new List<SpinPlayer>();
        while (selected.Count < numberOfPlayers)
        {
            if (i == _players.Count)
            {
                r = rnd.NextDouble();
                i = 0;
                continue;
            }
            
            var player = _players[i];
            var playerWeight = 1 - player.TimesChosen / Iterations;
            if (playerWeight > r)
            {
                selected.Add(player);
            }
        }
        
        return selected;
    }

    public Result<Round> StartSpin()
    {
        if (CurrentIteration > Iterations || State == SpinGameState.Finished)
        {
            State = SpinGameState.Finished;
            return new Error("The game is finished.");
        }

        State = SpinGameState.Spinning;
        CurrentIteration++;
        var challenge = _challenges[CurrentIteration - 1];

        var text = challenge.Text;
        if (challenge.ReadBeforeSpin)
        {
            text = "";
        }

        var round = new Round(text, challenge.Participants, SelectPlayers(challenge.Participants), _players.ToHashSet());
        return round;
    }

    public Result<string> StartRound()
    {
        if (CurrentIteration > Iterations || State == SpinGameState.Finished)
        {
            State = SpinGameState.Finished;
            return new Error("The game is finished.");
        }

        State = SpinGameState.RoundStarted;
        var challenge = _challenges[CurrentIteration - 1];
        return !challenge.ReadBeforeSpin 
            ? "Neste challenge kommer da noen blir valgt, vær klare!"
            : string.Empty;
    }

    public IList<Challenge> GetChallenges() => _challenges;
    
    public SpinGame AsCopy()
    {
        State = SpinGameState.ChallengesClosed;
        IsCopy = true;
        return this;
    }

    public static SpinGame Create(string name, int hostId, Category? category = Category.Random, IList<Challenge>? challenges = null)
        => new ()
        {
            Category = category ?? Category.Random,
            State = SpinGameState.Initialized,
            Name = name,
            Iterations = challenges?.Count ?? 0,
            CurrentIteration = 0,
            HostId = hostId,
            _challenges = challenges ?? [],
        };

    public override void SetUniversalId() => UniversalId = int.Parse("2" + Id);
}

// Extension for SpinGame
public static class SpinGameExtensions
{
    public static SpinGame PartialCopy(this SpinGame spinGame, int hostId)
        => SpinGame.Create(
            category: spinGame.Category,
            name: spinGame.Name,
            hostId: hostId,
            challenges: spinGame.GetChallenges()
        ).AsCopy();
}
