using tero.session.src.Core;
using tero.session.src.Features.Quiz;
using tero.session.src.Features.Spin;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddSignalR();
services.AddLogging();

// Add HybridCache
services.AddHybridCache();

// Configure Auth0Client
var auth0Options = builder.Configuration.GetSection("Auth0").Get<Auth0Options>()!;
services.AddHttpClient("Auth0Client", client =>
{
    client.BaseAddress = new Uri(auth0Options.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure PlatformClient
var platformOptions = builder.Configuration.GetSection("Platform").Get<PlatformOptions>()!;
services.AddHttpClient("PlatformClient", client =>
{
    client.BaseAddress = new Uri(platformOptions.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

services.AddCoreServices(builder.Configuration);

var app = builder.Build();

app.MapControllers();

app.AddQuizHub();
app.AddSpinHub();

// Health check for tero-platform
app.MapGet("/health", () => "OK");

app.Run();