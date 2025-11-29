using tero.session.src.Core;
using tero.session.src.Features.Auth;
using tero.session.src.Features.Platform;
using tero.session.src.Features.Quiz;
using tero.session.src.Features.Spin;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

services.AddControllers();
services.AddSignalR();
services.AddLogging();

// Add custom services
services.AddAuthServices(config);
services.AddPlatformServices(config);
services.AddCoreServices(config);

var app = builder.Build();
app.MapControllers();

// Add custom hubs
app.AddQuizHub();
app.AddSpinHub();
// Health check for tero-platform
app.MapGet("/health", () => "OK");

app.Run();