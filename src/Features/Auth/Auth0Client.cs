using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using tero.session.src.Core;
using tero.session.src.Features.Platform;

namespace tero.session.src.Features.Auth;

public class Auth0Client(IHttpClientFactory httpClientFactory, ILogger<Auth0Client> logger, IOptions<Auth0Options> options, PlatformClient platformClient)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(Auth0Client));
    private readonly Auth0Options _options = options.Value;
    private readonly SemaphoreSlim _semLock = new(1, 1);
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
        catch (JsonException error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("FetchM2MToken")
                .WithDescription("JsonException while fetching Auth0 M2M token")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(FetchM2MToken));
            return Error.Json;
        }
        catch (HttpRequestException error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("FetchM2MToken")
                .WithDescription("HttpRequestException while fetching Auth0 M2M token")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(FetchM2MToken));
            return Error.Http;
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("FetchM2MToken")
                .WithDescription("Unexpected exception while fetching Auth0 M2M token")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(FetchM2MToken));
            return Error.System;
        }
    }

    public async ValueTask<Result<string, Error>> GetToken()
    {
        try
        {
            await _semLock.WaitAsync();

            if (_cachedToken.IsValid())
            {
                return _cachedToken.Token;
            }

            var result = await FetchM2MToken();
            if (result.IsErr())
            {
                logger.LogError("Failed to fetch auth token from auth0");
                return result.Err();
            }

            var response = result.Unwrap();

            _cachedToken.SetToken(response.AccessToken);
            _cachedToken.SetExpiry(response.ExpiresIn);

            return response.AccessToken;
        }
        catch (Exception error)
        {
            var log = LogBuilder.New()
                .WithAction(LogAction.Other)
                .WithCeverity(LogCeverity.Critical)
                .WithFunctionName("GetToken")
                .WithDescription("Failed to get Auth0 token")
                .WithMetadata(error)
                .Build();

            platformClient.CreateSystemLogAsync(log);
            logger.LogError(error, nameof(GetToken));
            return Error.System;
        }
        finally
        {
            _semLock.Release();
        }
    }
}
