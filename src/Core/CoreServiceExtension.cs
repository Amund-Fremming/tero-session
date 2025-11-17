using tero_session.src.Core;

public static class CoreServiceExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<PlatformClient>();
        services.AddScoped<Auth0Client>();
        return services;
    }
}