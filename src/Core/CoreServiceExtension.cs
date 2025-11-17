using tero_session.src.Core;

public static class CoreServiceExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure options
        services.Configure<Auth0Options>(configuration.GetSection("Auth0"));
        services.Configure<PlatformOptions>(configuration.GetSection("Platform"));
        
        services.AddScoped<PlatformClient>();
        services.AddScoped<Auth0Client>();
        services.AddScoped<SessionCache>();
        
        return services;
    }
}