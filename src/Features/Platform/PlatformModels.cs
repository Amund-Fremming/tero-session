using System.Text.Json;
using System.Text.Json.Serialization;

namespace tero.session.src.Features.Platform;

public sealed record CacheInfo
{
    public int SpinSessionSize {get; set;}
    public int SpinManagerSize {get; set;}
    public int QuizSessionSize {get; set;}
    public int QuizManagerSize {get; set;}
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