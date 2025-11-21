using Newtonsoft.Json;

namespace tero.session.src.Core;

public sealed record SystemLogRequest
{
    public string? Description {get; set; }
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


public sealed record Result<T, E>
{
    private T? Data { get; set; }
    private E? Error { get; set; }

    private Result(T? data, E? error)
    {
        Data = data;
        Error = error;
    }

    public static Result<T, E> Ok(T data) => new(data, default!);

    public static Result<T, E> Err(E error) => new(default!, error);

    public static implicit operator Result<T, E>(T data) => new(data, default!);

    public static implicit operator Result<T, E>(E error) => new(default!, error);
    public E Err() => Error!;

    public bool IsErr() => Error is not null;
    public bool IsOk() => Data is not null;
    public T Unwrap()
    {
        if (Data is null)
        {
            throw new Exception("Cannot unwrap a error result");
        }

        return Data;
    }
}

public sealed record Option<T>(T Data)
{
    public static Option<T> Some(T data) => new(data);
    public static Option<T> None => new(default(T)!);

    public bool IsNone() => Data is null;
    public bool IsSome() => Data is not null;
}