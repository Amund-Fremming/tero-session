using Newtonsoft.Json;

public enum GameCategory
{
    [JsonProperty("casual")]
    Casual,
    [JsonProperty("random")]
    Random,
    [JsonProperty("ladies")]
    Ladies,
    [JsonProperty("boys")]
    Boys,
    [JsonProperty("default")]
    Default
}

public enum GameType
{
    Spin,
    Quiz
}

public record CachedToken(string Token, DateTime ExpiresAt)
{
    public CachedToken()
    {
        this.Token = string.Empty;
        this.ExpiresAt = DateTime.MinValue;
    }
}