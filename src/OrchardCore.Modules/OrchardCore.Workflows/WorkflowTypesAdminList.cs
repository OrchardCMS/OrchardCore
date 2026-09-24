namespace OrchardCore.Workflows;

/// <summary>
/// The admin list of workflow types rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="WorkflowTypesAdminListColumnProvider"/>.
/// </summary>
public static class WorkflowTypesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__WorkflowTypes</c> and <c>AdminListCell__WorkflowTypes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "WorkflowTypes";
}


/// <summary>
/// The admin list of workflow instances rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="WorkflowInstancesAdminListColumnProvider"/>.
/// </summary>
public static class WorkflowInstancesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__WorkflowInstances</c> and <c>AdminListCell__WorkflowInstances__{Column}</c> alternates.
    /// </summary>
    public const string Name = "WorkflowInstances";
}
