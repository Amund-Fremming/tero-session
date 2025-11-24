using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json;

public class SpinGamePlayer
{
    [JsonProperty("user_id")]
    public Guid UserId { get; private set; }

    [JsonProperty("times_chosen")]
    public int TimesChosen { get; private set; } = 0;

    public static SpinGamePlayer Create(Guid userId)
        => new()
        {
            UserId = userId,
            TimesChosen = 0,
        };

    public void IncTimesChosen() => TimesChosen++;
}

public enum SpinGameState
{
    Initialized,
    Closed,
    RoundStarted,
    Spinning,
    RoundFinished,
    Finished,
}