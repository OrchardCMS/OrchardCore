namespace OrchardCore.Modules;

/// <summary>
/// Identifies the server (node) this instance of the application runs on, among the servers that run the same
/// application, such as the servers of a web farm. Read from the <c>OrchardCore:Node</c> section of the application
/// configuration, not per tenant.
/// </summary>
public sealed class NodeOptions
{
    /// <summary>
    /// Gets or sets the identifier of this node. It must be unique among the running instances of the application,
    /// as it tells which instance does a piece of work that only one of them should do. It defaults to the machine
    /// name followed by the process identifier, which is unique even when several instances share a machine.
    /// </summary>
    public string Id { get; set; } = $"{System.Environment.MachineName}-{System.Environment.ProcessId}";
}
