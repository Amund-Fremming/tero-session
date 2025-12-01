using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tero.session.src.Features.Auth;

namespace tero.session.tests.Features.Auth;

public class Auth0ClientTests
{
    private readonly ILogger<Auth0Client> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Auth0Options _options;

    private Auth0Client CreateAuth0Client()
    {
        return new Auth0Client(
            _httpClientFactory,
            _logger,
            Options.Create(_options)
        );
    }

    public Auth0ClientTests()
    {
        // Load real configuration from appsettings
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _options = configuration.GetSection("Auth0").Get<Auth0Options>() 
            ?? throw new InvalidOperationException("Auth0 options not configured");
        
        _logger = new LoggerFactory().CreateLogger<Auth0Client>();
        _httpClientFactory = new DefaultHttpClientFactory(_options.BaseUrl);
    }

    private class DefaultHttpClientFactory(string baseUrl) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }
    }

    [Fact]
    public async Task GetToken_WhenCacheIsEmpty_ShouldFetchNewToken()
    {
        // Arrange
        var client = CreateAuth0Client();

        // Act
        var result = await client.GetToken();

        // Assert
        if (result.IsErr())
        {
            var error = result.Err();
            Assert.Fail($"Expected token but got error: {error}");
        }
        
        Assert.True(result.IsOk());
        var token = result.Unwrap();
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task GetToken_WhenCalledTwiceWithValidCache_ShouldReturnCachedToken()
    {
        // Arrange
        var client = CreateAuth0Client();

        // Act - First call
        var firstResult = await client.GetToken();
        
        // Act - Second call (should use cache)
        var secondResult = await client.GetToken();

        // Assert
        Assert.True(firstResult.IsOk());
        Assert.True(secondResult.IsOk());
        
        var firstToken = firstResult.Unwrap();
        var secondToken = secondResult.Unwrap();
        
        Assert.NotEmpty(firstToken);
        Assert.Equal(firstToken, secondToken); // Should return same cached token
    }

    [Fact]
    public async Task GetToken_MultipleConcurrentCalls_ShouldHandleThreadSafety()
    {
        // Arrange
        var client = CreateAuth0Client();

        // Act - Multiple concurrent calls
        var tasks = Enumerable.Range(0, 10).Select(_ => client.GetToken().AsTask()).ToArray();
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, r =>
        {
            Assert.True(r.IsOk());
            var token = r.Unwrap();
            Assert.NotEmpty(token);
        });
        
        // All tokens should be the same (from cache)
        var uniqueTokens = results.Select(r => r.Unwrap()).Distinct().ToList();
        Assert.Single(uniqueTokens); // Only one unique token despite concurrent calls
    }

}
