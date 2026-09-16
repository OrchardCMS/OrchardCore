namespace OrchardCore.RemoteManagement;

/// <summary>
/// Defines the stable identifiers used by the Orchard Core remote management protocol.
/// </summary>
public static class RemoteManagementConstants
{
    /// <summary>
    /// The current major protocol version.
    /// </summary>
    public const int ProtocolMajorVersion = 1;

    /// <summary>
    /// The current minor protocol version.
    /// </summary>
    public const int ProtocolMinorVersion = 0;

    /// <summary>
    /// The default public OpenID Connect client identifier used by the CLI.
    /// </summary>
    public const string CliClientId = "orchardcore-cli";

    /// <summary>
    /// The route that exposes the remote management bootstrap document.
    /// </summary>
    public const string BootstrapPath = ".well-known/orchardcore-management";

    /// <summary>
    /// The OpenAPI extension containing CLI command metadata.
    /// </summary>
    public const string CliExtensionName = "x-oc-cli";

    /// <summary>
    /// The response header identifying the active tenant's API feature and module revision.
    /// This opaque value is independent of the management protocol version.
    /// </summary>
    public const string ApiRevisionHeaderName = "OrchardCore-Api-Revision";

    /// <summary>
    /// The authentication scope required to call remote management APIs.
    /// </summary>
    public const string ManagementScope = "orchardcore.management";

}
