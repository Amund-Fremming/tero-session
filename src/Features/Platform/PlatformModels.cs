using System.Text.Json;
using System.Text.Json.Serialization;

namespace tero.session.src.Features.Platform;

public sealed record CacheInfo
{
    public int SpinSessionSize { get; set; }
    public int SpinManagerSize { get; set; }
    public int QuizSessionSize { get; set; }
    public int QuizManagerSize { get; set; }
}

public class PlatformOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}

public record GameSessionRequest
{
    [JsonPropertyName("payload")]
    public JsonElement Value { get; init; }
}

public sealed record SystemLogRequest(string? Description);

public sealed record CreateSyslogRequest
{
    [JsonPropertyName("action")]
    public LogAction? Action { get; set; }

    [JsonPropertyName("ceverity")]
    public LogCeverity? Ceverity { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("file_name")]
    public string? FileName { get; set; }

    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; set; }
}

public enum LogAction
{
    [JsonPropertyName("create")]
    Create,
    [JsonPropertyName("read")]
    Read,
    [JsonPropertyName("update")]
    Update,
    [JsonPropertyName("delete")]
    Delete,
    [JsonPropertyName("sync")]
    Sync,
    [JsonPropertyName("other")]
    Other,
}

public enum LogCeverity
{
    [JsonPropertyName("critical")]
    Critical,
    [JsonPropertyName("warning")]
    Warning,
    [JsonPropertyName("info")]
    Info,
}