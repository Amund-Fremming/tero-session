using System.Text.Json;
using System.Text.Json.Serialization;

namespace tero.session.src.Core;

public interface IJoinableSession
{
    public Option<Guid> AddUser(Guid userId);
}

public record GameSessionRequest
{
    [JsonPropertyName("payload")]
    public JsonElement Value { get; init; }
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
