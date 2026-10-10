namespace OrchardCore.Deployment;

/// <summary>
/// The admin list of deployment plans rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="DeploymentPlansAdminListColumnProvider"/>.
/// </summary>
public static class DeploymentPlansAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__DeploymentPlans</c> and <c>AdminListCell__DeploymentPlans__{Column}</c> alternates.
    /// </summary>
    public const string Name = "DeploymentPlans";
}
