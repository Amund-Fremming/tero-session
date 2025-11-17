using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace tero_session.src.Core;

public class PlatformClient(IHttpClientFactory httpClientFactory, ILogger<PlatformClient> logger, Auth0Client auth0Client, IOptions<PlatformOptions> options)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(PlatformClient));
    private readonly PlatformOptions _options = options.Value;

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
