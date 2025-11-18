using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace tero_session.src.Core;

public class Auth0Client(IHttpClientFactory httpClientFactory, ILogger<Auth0Client> logger, IOptions<Auth0Options> options)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(Auth0Client));
    private readonly Auth0Options _options = options.Value;
    private readonly object _locker = new();
    private readonly Cachedtoken _cachedToken = new();

    private async Task<Result<string, Exception>> GetAuthToken()
    {
        try
        {
            return "";
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error");
            return error;
        }
    }

    public async Task<Result<string, Exception>> GetCachedToken()
    {
        try
        {
            return "IMPLEMENT";
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error");
            return error;
        }
    }
}
