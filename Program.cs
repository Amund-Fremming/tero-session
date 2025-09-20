using tero_session.Common;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddSignalR();
services.AddLogging();

var app = builder.Build();

app.MapControllers();
app.MapHubs();
app.MapGet("/health", () => "OK");

app.Run();