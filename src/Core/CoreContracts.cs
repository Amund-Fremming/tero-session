namespace tero.session.src.Core;

public interface IJoinableSession
{
    public Option<Guid> AddUser(Guid userId);
}

public interface ICleanuppable<TSession>
{
    public TSession Cleanup(Guid userId);
}