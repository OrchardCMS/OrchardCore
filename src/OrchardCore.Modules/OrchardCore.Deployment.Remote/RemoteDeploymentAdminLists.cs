namespace OrchardCore.Deployment.Remote;

/// <summary>
/// The admin list of remote instances rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="RemoteInstancesAdminListColumnDriver"/>.
/// </summary>
public static class RemoteInstancesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__RemoteInstances</c> and <c>AdminListCell__RemoteInstances__{Column}</c> alternates.
    /// </summary>
    public const string Name = "RemoteInstances";
}

/// <summary>
/// The admin list of remote clients rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="RemoteClientsAdminListColumnDriver"/>.
/// </summary>
public static class RemoteClientsAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__RemoteClients</c> and <c>AdminListCell__RemoteClients__{Column}</c> alternates.
    /// </summary>
    public const string Name = "RemoteClients";
}
