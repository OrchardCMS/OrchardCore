using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.OpenId.Endpoints.Management;

/// <summary>Pages applications or scopes using the configured OpenID store's order.</summary>
public sealed class OpenIdListRequest
{
    /// <summary>Gets the zero-based offset, defaulting to zero.</summary>
    [FromQuery(Name = "skip"), Range(0, int.MaxValue)]
    public int? Skip { get; init; }
    /// <summary>Gets the page size, defaulting to 50 and limited to 200.</summary>
    [FromQuery(Name = "take"), Range(1, 200)]
    public int? Take { get; init; }
}

/// <summary>A page of redacted OpenID resource descriptions.</summary>
/// <typeparam name="T">The resource description type.</typeparam>
public sealed class OpenIdListResponse<T>
{
    /// <summary>Gets the applied offset.</summary>
    public int Skip { get; init; }
    /// <summary>Gets the applied page size.</summary>
    public int Take { get; init; }
    /// <summary>Gets the total number of stored resources before paging.</summary>
    public long TotalCount { get; init; }
    /// <summary>Gets this page's resource descriptions.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];
}

/// <summary>Describes an application without credentials, keys, custom properties or private settings.</summary>
public sealed class OpenIdApplicationResponse
{
    /// <summary>Gets the physical application identifier used by the admin UI.</summary>
    public string Id { get; init; }
    /// <summary>Gets the client identifier used for authentication.</summary>
    public string ClientId { get; init; }
    /// <summary>Gets the configured display name.</summary>
    public string DisplayName { get; init; }
    /// <summary>Gets the configured client type.</summary>
    public string ClientType { get; init; }
    /// <summary>Gets the configured application type.</summary>
    public string ApplicationType { get; init; }
    /// <summary>Gets the configured consent type.</summary>
    public string ConsentType { get; init; }
    /// <summary>Gets the assigned Orchard role names.</summary>
    public IReadOnlyList<string> Roles { get; init; } = [];
    /// <summary>Gets the registered OpenIddict permissions, including grants and scopes.</summary>
    public IReadOnlyList<string> Permissions { get; init; } = [];
    /// <summary>Gets the registered OpenIddict requirements.</summary>
    public IReadOnlyList<string> Requirements { get; init; } = [];
    /// <summary>Gets the registered redirect URIs.</summary>
    public IReadOnlyList<string> RedirectUris { get; init; } = [];
    /// <summary>Gets the registered post-logout redirect URIs.</summary>
    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];
}

/// <summary>Describes a scope without custom properties.</summary>
public sealed class OpenIdScopeResponse
{
    /// <summary>Gets the physical scope identifier used by the admin UI.</summary>
    public string Id { get; init; }
    /// <summary>Gets the scope name used in authorization requests.</summary>
    public string Name { get; init; }
    /// <summary>Gets the configured display name.</summary>
    public string DisplayName { get; init; }
    /// <summary>Gets the configured description.</summary>
    public string Description { get; init; }
    /// <summary>Gets the registered resource identifiers.</summary>
    public IReadOnlyList<string> Resources { get; init; } = [];
}
