using Microsoft.Extensions.Options;
using tero_session.src.Core;
using tero_session.src.Features.Quiz;
using tero_session.src.Features.Spin;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddSignalR();
services.AddLogging();

// Configure Auth0 options
services.Configure<Auth0Options>(builder.Configuration.GetSection("Auth0"));
services.AddHttpClient("Auth0Client", (serviceProvider, client) =>
{
    var auth0Options = serviceProvider.GetRequiredService<IOptions<Auth0Options>>().Value;
    client.BaseAddress = new Uri(auth0Options.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure Platform options
services.Configure<PlatformOptions>(builder.Configuration.GetSection("Platform"));
services.AddHttpClient("PlatformClient", (serviceProvider, client) =>
{
    var platformOptions = serviceProvider.GetRequiredService<IOptions<PlatformOptions>>().Value;
    client.BaseAddress = new Uri(platformOptions.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

services.AddCoreServices();

var app = builder.Build();

app.MapControllers();

app.AddQuizHub();
app.AddSpinHub();

// Health check for tero-platform
app.MapGet("/health", () => "OK");

app.Run();