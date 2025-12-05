namespace tero.session.src.Features.Platform;

public static class PlatformServiceExtension
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services, IConfiguration configuration)
    {
        var platformOptions = configuration.GetSection("Platform").Get<PlatformOptions>()!;
        services.AddSingleton(platformOptions);

        services.AddHttpClient("PlatformClient", client =>
        {
            client.BaseAddress = new Uri(platformOptions.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddSingleton<PlatformClient>();

        return services;
    }
}