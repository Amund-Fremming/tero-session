using System.ComponentModel.DataAnnotations;

namespace tero_session.src.Features.Spin;

// SpinGameState enum
public enum SpinGameState
{
    Initialized,
    ChallengesClosed,
    RoundStarted,
    Spinning,
    RoundFinished,
    Finished,
}

// Challenge class
public class Challenge
{
    [Key]
    public int Id { get; }

    public int GameId { get; private set; }
    public int Participants { get; private set; }
    
    [MaxLength(100)]
    public string Text { get; private set; } = string.Empty;
    public bool ReadBeforeSpin { get; private set; }

    private Challenge()
    { }

    public Challenge EmptyText()
    {
        Text = string.Empty;
        return this;
    }

    public static Challenge Create(int gameId, int participants, string text, bool readBeforeSpin = false)
        => new()
        {
            GameId = gameId,
            Participants = participants,
            Text = text,
            ReadBeforeSpin = readBeforeSpin,
        };
}

// SpinPlayer class
public class SpinPlayer
{
    [Key]
    public int Id { get; init; }

    public int GameId { get; init; }
    public int UserId { get; init; }
    public int TimesChosen { get; set; } = 0;
    public bool Active { get; private set; }

    public SpinGame SpinGame { get; private set; } = default!;
    public UserBase User { get; private set; } = default!;

    private SpinPlayer()
    { }

    public void Chosen() => TimesChosen++;

    public void SetActive(bool active) => Active = active;

    public static SpinPlayer Create(int gameId, int userId)
        => new()
        {
            GameId = gameId,
            UserId = userId,
            Active = true,
        };
}

// Round record
public sealed record Round(string ChallengeText, int RoundParticipants, List<SpinPlayer> AllPlayers, HashSet<SpinPlayer> SelectedPlayers);
