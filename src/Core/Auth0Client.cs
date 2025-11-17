using Microsoft.AspNetCore.Mvc;

namespace tero_session.src.Core;

public class Auth0Client(IHttpClientFactory httpClientFactory, ILogger<Auth0Client> logger)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(Auth0Client));

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
