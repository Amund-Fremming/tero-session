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

public record CachedToken
{
    public string Token {get; set;}
    private DateTime ExpiresAt {get; set;}

    public CachedToken()
    {
        Token = string.Empty;
        ExpiresAt = DateTime.MinValue;
    }

    public bool IsValid()
    {
        if(ExpiresAt < DateTime.Now || Token == string.Empty)
        {
            return false;
        }

        return true;
    }
}


public sealed record Result<T, E>
{
    private T? Data {get; set;}
    private E? Error {get; set;}

    private Result(T? data, E? error)
    {
        Data = data;
        Error = error;
    }

    public static Result<T, E> Ok(T data) => new(data, default!);

    public static Result<T, E> Err(E error) => new(default!, error);

    public static implicit operator Result<T, E>(T data) => new(data, default!);

    public static implicit operator Result<T, E>(E error) => new(default!, error);

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