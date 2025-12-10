namespace tero.session.src.Features.Quiz;

public static class QuizServiceExtension
{
    public static WebApplication AddQuizHub(this WebApplication app)
    {
        app.MapHub<QuizHub>("hubs/quiz");
        return app;
    }
}