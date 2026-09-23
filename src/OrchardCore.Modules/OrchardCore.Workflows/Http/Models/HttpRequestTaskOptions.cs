namespace OrchardCore.Workflows.Http.Models;

/// <summary>
/// Configures destination restrictions for the HTTP Request workflow task.
/// </summary>
public sealed class HttpRequestTaskOptions
{
    /// <summary>
    /// The shell configuration section for HTTP Request workflow task options.
    /// </summary>
    public const string ConfigurationSection = "OrchardCore_Workflows:HttpRequestTask";

    /// <summary>
    /// Gets or sets whether destinations must match an explicitly configured host, address, or network.
    /// </summary>
    public bool AllowOnlyConfiguredDestinations { get; set; }

    /// <summary>
    /// Gets or sets the host names that may resolve to otherwise prohibited addresses.
    /// </summary>
    public string[] AllowedHosts { get; set; } = [];

    /// <summary>
    /// Gets or sets the otherwise prohibited IP addresses that may be used as destinations.
    /// </summary>
    public string[] AllowedIpAddresses { get; set; } = [];

    /// <summary>
    /// Gets or sets the otherwise prohibited IP networks, in CIDR notation, that may be used as destinations.
    /// </summary>
    public string[] AllowedIpNetworks { get; set; } = [];
}
