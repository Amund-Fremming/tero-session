using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tero.session.src.Core;
using tero.session.src.Features.Auth;
using tero.session.src.Features.Platform;

namespace tero.session.tests.Features.Platform;

public class PlatformClientTests
{
    private readonly ILogger<PlatformClient> _logger;
    private readonly ILogger<Auth0Client> _auth0Logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Auth0Options _auth0Options;
    private readonly PlatformOptions _platformOptions;

    public PlatformClientTests()
    {
        // Load real configuration from appsettings
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _auth0Options = configuration.GetSection("Auth0").Get<Auth0Options>()
            ?? throw new InvalidOperationException("Auth0 options not configured");

        _platformOptions = configuration.GetSection("Platform").Get<PlatformOptions>()
            ?? throw new InvalidOperationException("Platform options not configured");

        _logger = new LoggerFactory().CreateLogger<PlatformClient>();
        _auth0Logger = new LoggerFactory().CreateLogger<Auth0Client>();
        _httpClientFactory = new MultiClientHttpFactory(_auth0Options.BaseUrl, _platformOptions.BaseUrl);
    }

    private PlatformClient CreatePlatformClient()
    {
        var auth0Client = new Auth0Client(
            _httpClientFactory,
            _auth0Logger,
            Options.Create(_auth0Options)
        );

        return new PlatformClient(
            _httpClientFactory,
            _logger,
            auth0Client
        );
    }

    private class MultiClientHttpFactory(string auth0BaseUrl, string platformBaseUrl) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return name switch
            {
                nameof(Auth0Client) => new HttpClient { BaseAddress = new Uri(auth0BaseUrl) },
                nameof(PlatformClient) => new HttpClient { BaseAddress = new Uri(platformBaseUrl) },
                _ => new HttpClient()
            };
        }
    }

    [Fact]
    public async Task CreateSystemLog_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var client = CreatePlatformClient();
        var request = new SystemLogRequest("Test system log from integration test");

        // Act
        var result = await client.CreateSystemLog(request);

        // Assert
        if (result.IsErr())
        {
            var error = result.Err();
            Assert.Fail($"Expected success but got error: {error}");
        }

        Assert.True(result.IsOk());
    }
}