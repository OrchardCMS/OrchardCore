using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.OpenId.ViewModels;

/// <summary>Contains the supported RFC 7591 public-client registration metadata.</summary>
public sealed class McpClientRegistrationRequest
{
    /// <summary>Gets or sets the unverified name shown during user consent.</summary>
    [JsonPropertyName("client_name"), StringLength(100)]
    public string ClientName { get; set; }

    /// <summary>Gets or sets the exact OAuth redirect URIs.</summary>
    [JsonPropertyName("redirect_uris"), Required, MinLength(1), MaxLength(8)]
    public string[] RedirectUris { get; set; } = [];

    /// <summary>Gets or sets the supported grants requested by the client.</summary>
    [JsonPropertyName("grant_types"), MaxLength(2)]
    public string[] GrantTypes { get; set; } = ["authorization_code"];

    /// <summary>Gets or sets the requested response types.</summary>
    [JsonPropertyName("response_types"), MaxLength(1)]
    public string[] ResponseTypes { get; set; } = ["code"];

    /// <summary>Gets or sets the token endpoint authentication method; only none is supported.</summary>
    [JsonPropertyName("token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; set; } = "none";

    /// <summary>Gets or sets the space-separated requested scopes.</summary>
    [JsonPropertyName("scope"), StringLength(256)]
    public string Scope { get; set; } = "orchardcore.management";
}
