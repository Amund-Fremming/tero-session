using tero_session.Quiz;
using tero_session.Spin;

namespace tero_session.Common;

public static class HubBuilderExtension
{
    public static WebApplication MapHubs(this WebApplication app)
    {
        const string hubPrefix = "/hub";
        
        app.MapHub<QuizHub>($"{hubPrefix}/quiz");
        app.MapHub<SpinHub>($"{hubPrefix}/spin");
        
        return app;
    }
}