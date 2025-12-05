namespace tero.session.src.Features.Auth;

public static class AuthServiceExtension
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        var auth0Options = configuration.GetSection("Auth0").Get<Auth0Options>()!;
        services.AddSingleton(auth0Options);

        services.AddHttpClient("Auth0Client", client =>
        {
            client.BaseAddress = new Uri(auth0Options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.Configure<Auth0Options>(configuration.GetSection("Auth0"));
        services.AddSingleton<Auth0Client>();

        return services;
    }
}