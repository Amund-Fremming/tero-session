using System.Runtime.CompilerServices;
using tero.session.src.Features.Platform;

namespace tero.session.src.Core;

public static class CoreExtensions
{
    public static List<T> Shuffle<T>(this List<T> list)
    {
        var random = new Random();

        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    public static async Task LogToBackend(
        this PlatformClient platformClient,
        Exception exception,
        LogCeverity severity,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = ""
    )
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var log = LogBuilder.New()
                .WithCeverity(severity)
                .WithDescription($"{exception.GetType().Name}: {exception.Message}")
                .WithFileName(fileName)
                .WithMetadata(new
                {
                    Method = memberName,
                    ExceptionType = exception.GetType().FullName,
                    StackTrace = exception.StackTrace
                })
                .Build();

            await platformClient.CreateSystemLog(log);
        }
        catch
        {
            // Silently fail to avoid infinite loops and cascading failures
        }
    }

    public static void LogToBackendFireAndForget(
        this PlatformClient? platformClient,
        Exception exception,
        LogCeverity severity,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = ""
    )
    {
        if (platformClient == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await platformClient.LogToBackend(exception, severity, filePath, memberName);
            }
            catch
            {
                // Silently fail to avoid infinite loops and cascading failures
            }
        });
    }
}