using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using tero.session.src.Core;
using tero.session.src.Features.Auth;

namespace tero.session.src.Features.Platform;

public class PlatformClient(IHttpClientFactory httpClientFactory, ILogger<PlatformClient> logger, Auth0Client auth0Client)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(PlatformClient));
    private readonly JsonSerializerOptions _jsonOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public async Task<Result<Error>> PersistGame()
    {
        try
        {
            var result = await auth0Client.GetToken();
            if (result.IsErr())
            {
                return result.Err();
            }

            var token = result.Unwrap();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsync("/games/persist", null);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to persist game, status code: {StatusCode}", response.StatusCode);
                return Error.Http;
            }

            return Result<Error>.Ok;
        }
        catch (HttpRequestException error)
        {
            logger.LogError(error, nameof(PersistGame));
            // Cannot log to backend here to avoid potential infinite loop
            return Error.Http;
        }
        catch (Exception error)
        {
            logger.LogError(error, nameof(PersistGame));
            // Cannot log to backend here to avoid potential infinite loop
            return Error.System;
        }
    }

    public async Task<Result<Error>> CreateSystemLog(CreateSyslogRequest request)
    {
        try
        {
            var result = await auth0Client.GetToken();
            if (result.IsErr())
            {
                return result.Err();
            }

            var token = result.Unwrap();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);



            var json = JsonSerializer.Serialize(request, _jsonOptions);
            logger.LogDebug("Sending system log request: {Json}", json);

            var content = new StringContent(
               json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("/logs", content);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to create system log, status code: {StatusCode}, response: {Response}", response.StatusCode, responseBody);
                return Error.Http;
            }

            return Result<Error>.Ok;
        }
        catch (HttpRequestException error)
        {
            logger.LogError(error, nameof(CreateSystemLog));
            // Cannot log to backend here to avoid infinite loop
            return Error.Http;
        }
        catch (Exception error)
        {
            logger.LogError(error, "Error creating system log");
            // Cannot log to backend here to avoid infinite loop
            return Error.System;
        }
    }
}
