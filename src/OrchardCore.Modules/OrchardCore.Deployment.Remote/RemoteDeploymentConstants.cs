namespace OrchardCore.Deployment.Remote;

/// <summary>
/// The names of the breadcrumbs rendered by the remote deployment screens. A module adds a node to one of these trails
/// by registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class RemoteDeploymentConstants
{
    /// <summary>
    /// The breadcrumb of the remote instances list. It is named after the list of that screen.
    /// </summary>
    public const string Instances = "RemoteInstances";

    /// <summary>
    /// The breadcrumb of the remote instance creation screen.
    /// </summary>
    public const string InstancesCreate = "RemoteInstancesCreate";

    /// <summary>
    /// The breadcrumb of the remote instance edition screen.
    /// </summary>
    public const string InstancesEdit = "RemoteInstancesEdit";

    /// <summary>
    /// The breadcrumb of the remote clients list. It is named after the list of that screen.
    /// </summary>
    public const string Clients = "RemoteClients";

    /// <summary>
    /// The breadcrumb of the remote client edition screen.
    /// </summary>
    public const string ClientsEdit = "RemoteClientsEdit";
}
