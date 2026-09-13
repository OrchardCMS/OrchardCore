using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.OpenId.Endpoints.Management;

/// <summary>Replaces the settings exposed by the application editor, with credentials accepted only as input.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class OpenIdApplicationMutationRequest
{
    /// <summary>The stable client identifier.</summary>
    [Required]
    public string ClientId { get; init; }
    /// <summary>The application display name.</summary>
    [Required]
    public string DisplayName { get; init; }
    /// <summary>The public or confidential client type.</summary>
    [Required]
    public string ClientType { get; init; }
    /// <summary>The web or native application type.</summary>
    public string ApplicationType { get; init; } = "web";
    /// <summary>The explicit, implicit, external or systematic consent type.</summary>
    public string ConsentType { get; init; } = "explicit";
    /// <summary>The input credential; omission preserves an existing confidential credential.</summary>
    public string ClientSecret { get; init; }
    /// <summary>The space or comma separated absolute redirect URIs; omission clears them.</summary>
    public string RedirectUris { get; init; }
    /// <summary>The space or comma separated post-logout URIs; omission clears them.</summary>
    public string PostLogoutRedirectUris { get; init; }
    /// <summary>The replacement Orchard role names; omission clears them.</summary>
    public string[] Roles { get; init; } = [];
    /// <summary>The replacement allowed scope names; omission clears them.</summary>
    public string[] Scopes { get; init; } = [];
    /// <summary>Whether to allow password flow.</summary>
    public bool AllowPasswordFlow { get; init; }
    /// <summary>Whether to allow client credentials flow.</summary>
    public bool AllowClientCredentialsFlow { get; init; }
    /// <summary>Whether to allow authorization code flow.</summary>
    public bool AllowAuthorizationCodeFlow { get; init; }
    /// <summary>Whether to allow device authorization flow.</summary>
    public bool AllowDeviceAuthorizationFlow { get; init; }
    /// <summary>Whether to allow refresh token flow.</summary>
    public bool AllowRefreshTokenFlow { get; init; }
    /// <summary>Whether to allow hybrid flow.</summary>
    public bool AllowHybridFlow { get; init; }
    /// <summary>Whether to allow implicit flow.</summary>
    public bool AllowImplicitFlow { get; init; }
    /// <summary>Whether to allow logout endpoint.</summary>
    public bool AllowLogoutEndpoint { get; init; }
    /// <summary>Whether to allow introspection endpoint.</summary>
    public bool AllowIntrospectionEndpoint { get; init; }
    /// <summary>Whether to allow revocation endpoint.</summary>
    public bool AllowRevocationEndpoint { get; init; }
    /// <summary>Whether to require proof key for code exchange.</summary>
    public bool RequireProofKeyForCodeExchange { get; init; }
    /// <summary>Whether to require pushed authorization requests.</summary>
    public bool RequirePushedAuthorizationRequests { get; init; }

    internal OpenIdApplicationSettings ToSettings() => new()
    {
        ClientId = ClientId,
        DisplayName = DisplayName,
        Type = ClientType,
        ApplicationType = ApplicationType,
        ConsentType = ConsentType,
        ClientSecret = ClientSecret,
        RedirectUris = RedirectUris,
        PostLogoutRedirectUris = PostLogoutRedirectUris,
        Roles = Roles,
        Scopes = Scopes,
        AllowPasswordFlow = AllowPasswordFlow,
        AllowClientCredentialsFlow = AllowClientCredentialsFlow,
        AllowAuthorizationCodeFlow = AllowAuthorizationCodeFlow,
        AllowDeviceAuthorizationFlow = AllowDeviceAuthorizationFlow,
        AllowRefreshTokenFlow = AllowRefreshTokenFlow,
        AllowHybridFlow = AllowHybridFlow,
        AllowImplicitFlow = AllowImplicitFlow,
        AllowLogoutEndpoint = AllowLogoutEndpoint,
        AllowIntrospectionEndpoint = AllowIntrospectionEndpoint,
        AllowRevocationEndpoint = AllowRevocationEndpoint,
        RequireProofKeyForCodeExchange = RequireProofKeyForCodeExchange,
        RequirePushedAuthorizationRequests = RequirePushedAuthorizationRequests,
    };
}
