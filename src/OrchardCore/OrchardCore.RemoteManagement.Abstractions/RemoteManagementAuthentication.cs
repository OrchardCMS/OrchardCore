namespace OrchardCore.RemoteManagement;

/// <summary>
/// Describes the authentication methods supported by a remote management endpoint.
/// </summary>
public sealed class RemoteManagementAuthentication
{
    /// <summary>
    /// Gets or sets the OpenID Connect authority.
    /// </summary>
    public Uri Authority { get; set; }

    /// <summary>
    /// Gets or sets the public CLI client identifier. It is null when the CLI feature is disabled.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets the supported OAuth grant types.
    /// </summary>
    public IList<string> GrantTypes { get; } = [];

    /// <summary>
    /// Gets the scopes requested by the CLI.
    /// </summary>
    public IList<string> Scopes { get; } = [];
}
