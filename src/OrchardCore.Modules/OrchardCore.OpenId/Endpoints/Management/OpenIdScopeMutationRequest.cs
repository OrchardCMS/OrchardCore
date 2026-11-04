using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchardCore.OpenId.Endpoints.Management;

/// <summary>The editable fields of a registered OpenID scope.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class OpenIdScopeMutationRequest
{
    /// <summary>Gets the required scope name. Updates cannot rename a scope.</summary>
    [Required]
    public string Name { get; init; }
    /// <summary>Gets the required display name, as in the admin editor.</summary>
    [Required]
    public string DisplayName { get; init; }
    /// <summary>Gets the optional description. Omission clears the description.</summary>
    public string Description { get; init; }
    /// <summary>Gets the complete resource list. Omission or an empty array clears it; null is invalid.</summary>
    public string[] Resources { get; init; } = [];
}
