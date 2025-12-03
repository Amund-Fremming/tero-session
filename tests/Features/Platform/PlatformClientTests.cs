using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tero.session.src.Core;
using tero.session.src.Features.Auth;
using tero.session.src.Features.Platform;
using Xunit.Abstractions;

namespace tero.session.tests.Features.Platform;

public class PlatformClientTests
{
    private readonly ILogger<PlatformClient> _logger;
    private readonly ILogger<Auth0Client> _auth0Logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Auth0Options _auth0Options;
    private readonly PlatformOptions _platformOptions;
    private readonly ITestOutputHelper _output;

    public PlatformClientTests(ITestOutputHelper output)
    {
        _output = output;

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

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        _logger = loggerFactory.CreateLogger<PlatformClient>();
        _auth0Logger = loggerFactory.CreateLogger<Auth0Client>();
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
        var request = new CreateSyslogRequest()
        {
            Description = "Heya",
            Ceverity = LogCeverity.Info
        };

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
public class XunitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new XunitLogger(output, categoryName);
    public void Dispose() { }
}

public class XunitLogger(ITestOutputHelper output, string categoryName) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        output.WriteLine($"[{logLevel}] {categoryName}: {formatter(state, exception)}");
        if (exception != null)
        {
            output.WriteLine(exception.ToString());
        }
    }
}
