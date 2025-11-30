using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using tero.session.src.Core;

namespace tero.session.src.Features.Auth;

public class Auth0Client(IHttpClientFactory httpClientFactory, ILogger<Auth0Client> logger, IOptions<Auth0Options> options)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(Auth0Client));
    private readonly Auth0Options _options = options.Value;
    private readonly object _lock = new();
    private readonly CachedToken _cachedToken = new();

    private async Task<Result<M2MTokenResponse, Error>> FetchM2MToken()
    {
        try
        {
            M2MTokenRequest payload = new()
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Audience = _options.Audience,
                GrantType = "client_credentials"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload).ToString(),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/oauth/token", content);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Response from auth0 was unsuccessful");
                return Error.Upstream;
            }

            var json = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<M2MTokenResponse>(json);

            if (token is null)
            {
                logger.LogError("Auth token response was null");
                return Error.NullReference;
            }

            return token;
        }
        catch(JsonException error)
        {
            logger.LogError(error, nameof(FetchM2MToken));
            return Error.Json;
        }
        catch (HttpRequestException error)
        {
            logger.LogError(error, nameof(FetchM2MToken));
            return Error.Http;
        }
        catch (Exception error)
        {
            logger.LogError(error, nameof(FetchM2MToken));
            return Error.System;
        }
    }

    public async ValueTask<Result<string, Error>> GetToken()
    {
        try
        {
            lock (_lock)
            {
                if (_cachedToken.IsValid())
                {
                    return _cachedToken.Token;
                }
            }

            var result = await FetchM2MToken();
            if (result.IsErr())
            {
                logger.LogError("Failed to fetch auth token from auth0");
                return result.Err();
            }

            var response = result.Unwrap();
            lock (_lock)
            {
                _cachedToken.SetToken(response.AccessToken);
                _cachedToken.SetExpiry(response.ExpiresIn);
            }

            return response.AccessToken;
        }
        catch (Exception error)
        {
            logger.LogError(error,nameof(GetToken));
            return Error.System;
        }
    }
}
