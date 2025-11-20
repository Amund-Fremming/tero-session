namespace tero.session.src.Features.Spin;

public static class SpinServiceExtension
{
    public static WebApplication AddSpinHub(this WebApplication app)
    {
        app.MapHub<SpinHub>($"hub/Spin)");
        return app;
    }
}