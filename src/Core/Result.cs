
namespace tero_session.src.Core;

public sealed record Result<T, E>(T Data, E Error)
{
    public static Result<T, E> Ok(T data) => new(data, default!);

    public static Result<T, E> Err(E error) => new(default!, error);

    public static implicit operator Result<T, E>(T data) => new(data, default!);

    public static implicit operator Result<T, E>(E error) => new(default!, error);
}

public sealed record Option<T>(T Data)
{
    public static Option<T> Some(T data) => new(data);
    public static Option<T> None() => new(default(T)!);
}