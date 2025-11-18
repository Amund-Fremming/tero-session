using tero_session.src.Core;
using tero_session.src.Features.Quiz;
using tero_session.src.Features.Spin;

public static class CoreServiceExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Auth0Options>(configuration.GetSection("Auth0"));
        services.Configure<PlatformOptions>(configuration.GetSection("Platform"));

        services.AddScoped<PlatformClient>();
        services.AddScoped<Auth0Client>();

        services.AddScoped<SessionCache<SpinSession>>();
        services.AddScoped<SessionCache<QuizSession>>();

        return services;
    }
}