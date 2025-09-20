using System.Text.Json;
using System.Text.Json.Serialization;

namespace tero_session.Common;

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