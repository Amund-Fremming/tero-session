using tero_session.src.Core;

public static class CoreServiceExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<PlatformClient>();
        services.AddScoped<Auth0Client>();
        services.AddScoped<SessionCache>();
        
#pragma warning disable EXTEXP0018
        services.AddHybridCache();
#pragma warning restore EXTEXP0018
        
        return services;
    }
}