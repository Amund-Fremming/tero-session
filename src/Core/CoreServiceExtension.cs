namespace tero.session.src.Core;

public static class CoreServiceExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Auth0Options>(configuration.GetSection("Auth0"));
        services.Configure<PlatformOptions>(configuration.GetSection("Platform"));

        services.AddScoped<PlatformClient>();
        services.AddScoped<Auth0Client>();

        services.AddSingleton<HubConnectionCache>();

        return services;
    }
}