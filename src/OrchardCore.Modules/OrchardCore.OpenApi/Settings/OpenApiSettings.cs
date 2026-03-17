namespace OrchardCore.OpenApi.Settings;

public class OpenApiSettings
{
    public bool EnableSwaggerUI { get; set; } = true;

    public bool EnableReDocUI { get; set; }

    public bool EnableScalarUI { get; set; }

    /// <summary>
    /// The OAuth2 authorization endpoint URL (e.g., "/connect/authorize").
    /// </summary>
    public string AuthorizationUrl { get; set; }

    /// <summary>
    /// The OAuth2 token endpoint URL (e.g., "/connect/token").
    /// </summary>
    public string TokenUrl { get; set; }

    /// <summary>
    /// The OAuth2 client ID used by the API documentation UIs.
    /// </summary>
    public string OAuthClientId { get; set; }

    /// <summary>
    /// A space-separated list of OAuth2 scopes (e.g., "openid profile email").
    /// </summary>
    public string OAuthScopes { get; set; }
}
