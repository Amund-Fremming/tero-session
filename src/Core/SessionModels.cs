using System.Text.Json;
using System.Text.Json.Serialization;

namespace tero_session.src.Core;

public record GameSessionRequest
{
    [JsonPropertyName("game_type")]
    public GameType GameType { get; init; }

    [JsonPropertyName("payload")]
    public JsonElement Payload { get; init; }
}

public enum GameType
{
    Spin,
    Quiz
}

public class Auth0Options
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class PlatformOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}

// Category enum
public enum Category
{
    Random,
    Friendly,
    Dirty,
    Flirty,
    ForTheBoys,
    ForTheGirls,
}

// Shared user models
public sealed class RegisteredUser : tero_session.src.Features.Spin.UserBase
{
    public string Auth0Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    private RegisteredUser()
    { }

    public static RegisteredUser Create(string authId, string name, string email)
        => new()
        {
            Guid = Guid.NewGuid(),
            LastActive = DateTime.Now,
            Auth0Id = authId,
            Name = name,
            Email = email
        };
}

public sealed class GuestUser : tero_session.src.Features.Spin.UserBase
{
    private GuestUser()
    { }

    public static GuestUser Create() => new() { Guid = Guid.NewGuid(), LastActive = DateTime.UtcNow };
}

// Result pattern classes
public sealed record Error(string Message, Exception? Exception = null);

public sealed record Result<T>(T Data, Error Error, EmptyResult? EmptyResult = null) : IResult, IResult<T>
{
    public bool IsEmpty => EmptyResult != null;
    public bool IsError => Error is not null && Error.Message is not null;

    public string Message => Error!.Message;

    public static Result<T> Ok(T data) => new(data, null!);

    public static implicit operator Result<T>(T data) => new(data, null!);

    public static implicit operator Result<T>(Error error) => new(default!, error);

    public static implicit operator Result<T>(EmptyResult emptyResult) => new(default!, new Error("Entity does not exist"), emptyResult);
}

public sealed record Result(Error Error) : IResult
{
    public bool IsError => Error is not null && Error.Exception is not null;

    public string Message => Error == null ? "No error message present." : Error.Message;

    public static Result Ok => new(Error: null!);

    public static implicit operator Result(Error error) => new(error);

    public static Result operator &(Result left, Result right)
    {
        if (left.IsError)
        {
            return left;
        }

        return right;
    }
}

public interface IResult
{
    bool IsError { get; }
    string Message { get; }
}

public interface IResult<T> : IResult
{
    T Data { get; }
}

public sealed record EmptyResult();
