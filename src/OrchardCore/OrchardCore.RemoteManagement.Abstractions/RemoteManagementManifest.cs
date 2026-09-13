namespace OrchardCore.RemoteManagement;

/// <summary>
/// Describes an Orchard Core tenant's remote management protocol and capabilities.
/// </summary>
public sealed class RemoteManagementManifest
{
    /// <summary>
    /// Gets or sets the protocol major version.
    /// </summary>
    public int ProtocolMajorVersion { get; set; } = RemoteManagementConstants.ProtocolMajorVersion;

    /// <summary>
    /// Gets or sets the protocol minor version.
    /// </summary>
    public int ProtocolMinorVersion { get; set; } = RemoteManagementConstants.ProtocolMinorVersion;

    /// <summary>
    /// Gets or sets the Orchard Core product version.
    /// </summary>
    public string ProductVersion { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier. This value is omitted from anonymous bootstrap responses.
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// Gets or sets authentication metadata.
    /// </summary>
    public RemoteManagementAuthentication Authentication { get; set; } = new();

    /// <summary>
    /// Gets or sets the authenticated management manifest URL.
    /// </summary>
    public Uri ManagementManifestUrl { get; set; }

    /// <summary>
    /// Gets or sets the authenticated OpenAPI document URL.
    /// </summary>
    public Uri OpenApiUrl { get; set; }

    /// <summary>
    /// Gets or sets the OpenAPI document entity tag.
    /// </summary>
    public string OpenApiETag { get; set; }

    /// <summary>
    /// Gets the enabled management capabilities.
    /// </summary>
    public IList<RemoteManagementCapability> Capabilities { get; } = [];

    /// <summary>
    /// Gets or sets the JSON Schema dialect used by resource schemas.
    /// </summary>
    public Uri JsonSchemaDialect { get; set; }

    /// <summary>
    /// Gets or sets the oldest CLI version supported by the tenant.
    /// </summary>
    public string MinimumCliVersion { get; set; }

    /// <summary>
    /// Gets or sets the CLI version recommended by the tenant.
    /// </summary>
    public string RecommendedCliVersion { get; set; }

    /// <summary>
    /// Gets or sets the Orchard Core documentation search index URL.
    /// </summary>
    public Uri DocumentationIndexUrl { get; set; }
}
