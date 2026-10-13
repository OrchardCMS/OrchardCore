namespace OrchardCore.Deployment;

/// <summary>
/// The admin list of deployment plans rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="DeploymentPlansAdminListColumnDriver"/>.
/// </summary>
public static class DeploymentPlansAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__DeploymentPlans</c> and <c>AdminListCell__DeploymentPlans__{Column}</c> alternates.
    /// </summary>
    public const string Name = "DeploymentPlans";
}
