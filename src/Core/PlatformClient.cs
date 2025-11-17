using System.Runtime.CompilerServices;

namespace tero_session.src.Core;

public class PlatformClient(IHttpClientFactory httpClientFactory, ILogger<PlatformClient> logger, Auth0Client auth0Client)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(PlatformClient));

    public async Task<Result<bool, Exception>> PersistGame()
    {
        try
        {
            // TODO
            return true;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error");
            return error;
        }
    }

    public async Task<Result<bool, Exception>> FreeGameKey()
    {
        try
        {
            // TODO
            return true;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error");
            return error;
        }
    }
}
