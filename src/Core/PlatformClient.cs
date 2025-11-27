using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace tero.session.src.Core;

public class PlatformClient(IHttpClientFactory httpClientFactory, ILogger<PlatformClient> logger, Auth0Client auth0Client, IOptions<PlatformOptions> options)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(PlatformClient));
    private readonly PlatformOptions _options = options.Value;

    public async Task<Result<Exception>> PersistGame()
    {
        try
        {
            var result = await auth0Client.GetToken();
            if (result.IsErr())
            {
                return result.Err();
            }
            // TODO
            return Result<Exception>.Ok;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error");
            return error;
        }
    }

    public async Task<Result<Exception>> FreeGameKey(string key)
    {
        try
        {
            var result = await auth0Client.GetToken();
            if (result.IsErr())
            {
                return result.Err();
            }
            // TODO
            return Result<Exception>.Ok;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error");
            return error;
        }
    }

    public async Task<Result<Exception>> CreateSystemLog(SystemLogRequest request)
    {
        try
        {
            var result = await auth0Client.GetToken();
            if (result.IsErr())
            {
                return result.Err();
            }
            // TODO
            return Result<Exception>.Ok;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error");
            return error;
        }
    } 
}
