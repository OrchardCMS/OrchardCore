using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Media;

/// <summary>
/// The authentication scheme the Media API (the api/media endpoints, the tus upload endpoint and
/// the media SignalR hub) authenticates against. Exactly one is active at a time — never both — so
/// the Media Gallery works out of the box with the ambient admin cookie and can opt into bearer
/// tokens (OAuth2 + PKCE) once OpenID is configured.
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

    public MediaApiAuthenticationScheme AuthenticationScheme { get; set; } = MediaApiAuthenticationScheme.Cookie;
}

public static class MediaApiConstants
{
    /// <summary>The authorization policy the Media API endpoints, tus endpoint and hub require.</summary>
    public const string AuthorizationPolicyName = "MediaApi";

    /// <summary>The bearer forwarding scheme (resolves to OpenIddict validation when enabled).</summary>
    public const string ApiScheme = "Api";

    /// <summary>The same-origin admin cookie scheme.</summary>
    public static readonly string CookieScheme = IdentityConstants.ApplicationScheme;
}
