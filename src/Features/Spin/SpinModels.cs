using Newtonsoft.Json;

public class SpinGamePlayer
{
    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    [JsonProperty("times_chosen")]
    public string TimesChosen { get; set; } = string.Empty;
}