namespace OrchardCore.SignalR.Azure;

/// <summary>
/// The settings of the Azure SignalR Service backplane, from the <c>SignalR:Azure</c> configuration section.
/// </summary>
internal sealed class AzureSignalROptions
{
    public string ConnectionString { get; set; }

    public string ApplicationName { get; set; }
}
