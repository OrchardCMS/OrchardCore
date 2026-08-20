namespace OrchardCore.Media;

/// <summary>
/// The authentication scheme used by the Media API and TUS upload endpoint. Exactly one is active
/// at a time so the Media Gallery works out of the box with the ambient admin cookie and can opt
/// into bearer tokens (OAuth2 + PKCE) once OpenID is configured. The media SignalR hub accepts
/// either scheme.
/// </summary>
public enum MediaApiAuthenticationScheme
{
    /// <summary>Same-origin admin cookie, with antiforgery validation on mutations. The default.</summary>
    Cookie,

    /// <summary>OAuth2 bearer tokens via the "Api" scheme (requires OpenID Token Validation).</summary>
    Bearer,
}

public class MediaApiSettings
{
    public const string GroupId = "mediaApi";

    public MediaApiAuthenticationScheme AuthenticationScheme { get; set; }
}

public static class MediaApiConstants
{
    /// <summary>The authorization policy the Media API and TUS endpoints require.</summary>
    public const string AuthorizationPolicyName = "MediaApi";

    public const string HubAuthorizationPolicyName = "MediaHub";
}
