using Newtonsoft.Json;

namespace tero.session.src.Features.Auth;


public class Auth0Options
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}


public sealed record M2MTokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}

public sealed record M2MTokenRequest
{

    [JsonProperty("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonProperty("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;

    [JsonProperty("audience")]
    public string Audience { get; set; } = string.Empty;

    [JsonProperty("grant_type")]
    public string GrantType { get; set; } = string.Empty;

}


public record CachedToken
{
    public string Token { get; set; }
    private DateTime ExpiresAt { get; set; }

    public CachedToken()
    {
        Token = string.Empty;
        ExpiresAt = DateTime.MinValue;
    }

    public void SetToken(string token)
    {
        Token = token;
    }

    public void SetExpiry(int seconds)
    {
        var expiry = DateTime.Now.AddSeconds(seconds);
        ExpiresAt = expiry;
    }

    public bool IsValid()
    {
        if (ExpiresAt < DateTime.Now || Token == string.Empty)
        {
            return false;
        }

        return true;
    }
}