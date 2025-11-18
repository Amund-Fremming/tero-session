public interface ISessionCache<T>
{

    Task<Result<T, Exception>> Get(string key);

    Task<Result<bool, Exception>> Insert(string key, T value);
}

public interface IJoinableSession
{
    public void AddToSession(Guid userId);
}