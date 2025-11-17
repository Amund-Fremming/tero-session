using System.Text.Json;
using System.Text.Json.Serialization;

namespace tero_session.src.Core;

public record GameSessionRequest
{
    [JsonPropertyName("game_type")]
    public GameType GameType { get; init; }

    [JsonPropertyName("payload")]
    public JsonElement Payload { get; init; }
}

public enum GameType
{
    Spin,
    Quiz
}

public class Auth0Options
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class PlatformOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}
