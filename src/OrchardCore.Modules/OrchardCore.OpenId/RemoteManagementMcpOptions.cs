namespace OrchardCore.OpenId;

/// <summary>Controls automatic OAuth registration for tenant MCP clients.</summary>
public sealed class RemoteManagementMcpOptions
{
    /// <summary>Gets or sets whether compatible clients can register automatically.</summary>
    public bool AllowDynamicClientRegistration { get; set; } = true;

    /// <summary>Gets or sets the total application count above which automatic registration stops.</summary>
    public int MaximumApplications { get; set; } = 1000;
}
