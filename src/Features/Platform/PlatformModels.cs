using System.Text.Json;
using System.Text.Json.Serialization;

namespace tero.session.src.Features.Platform;

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