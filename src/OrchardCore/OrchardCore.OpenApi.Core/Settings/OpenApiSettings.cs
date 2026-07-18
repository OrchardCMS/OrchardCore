using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchardCore.OpenApi.Settings;

public class OpenApiSettings
{
    /// <summary>
    /// Whether the OpenAPI JSON schema endpoints can be accessed without authentication.
    /// When <c>false</c> (the default), the schema endpoints require the
    /// <c>ViewOpenApiContent</c> permission. The OpenApi generation recipes enable this
    /// explicitly so headless tools like NSwag can fetch the schema anonymously.
    /// </summary>
    public bool AllowAnonymousSchemaAccess { get; set; }

    /// <summary>
    /// The authentication type used by the API documentation UIs.
    /// </summary>
    [JsonConverter(typeof(SafeEnumConverter<OpenApiAuthenticationType>))]
    public OpenApiAuthenticationType AuthenticationType { get; set; }

    /// <summary>
    /// The OAuth2 authorization endpoint URL (e.g., "/connect/authorize").
    /// Only used with Authorization Code + PKCE flow.
    /// </summary>
    public string AuthorizationUrl { get; set; }

    /// <summary>
    /// The OAuth2 token endpoint URL (e.g., "/connect/token").
    /// </summary>
    public string TokenUrl { get; set; }

    /// <summary>
    /// Optional URL of the OpenID Connect server metadata document
    /// (e.g., "/.well-known/openid-configuration"). When provided, the configuration is
    /// validated against this document on save. The issuer is never inferred from the
    /// endpoint URLs, since the spec does not tie endpoint locations to the issuer.
    /// </summary>
    public string ServerMetadataUrl { get; set; }

    /// <summary>
    /// The OAuth2 client ID used by the API documentation UIs.
    /// </summary>
    public string OAuthClientId { get; set; }

    /// <summary>
    /// A space-separated list of OAuth2 scopes (e.g., "openid profile email").
    /// </summary>
    public string OAuthScopes { get; set; }
}

public enum OpenApiAuthenticationType
{
    None = 0,
    AuthorizationCodePkce = 1,
}

/// <summary>
/// A JSON converter that falls back to the default enum value when the stored
/// value is unknown, preventing deserialization errors after enum changes.
/// </summary>
internal sealed class SafeEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intValue))
        {
            if (Enum.IsDefined(typeof(T), intValue))
            {
                return (T)Enum.ToObject(typeof(T), intValue);
            }

            return default;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            if (Enum.TryParse<T>(stringValue, ignoreCase: true, out var result))
            {
                return result;
            }

            return default;
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}
