using tero.session.src.Features.Spin;

namespace tero.session.src.Core;

public interface IJoinableSession
{
    public Result<SpinSession, Error> AddUser(Guid userId);
}

public interface ICleanuppable<TSession>
{
    public TSession Cleanup(Guid userId);
}