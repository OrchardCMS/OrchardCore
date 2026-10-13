namespace OrchardCore.Workflows;

/// <summary>
/// The admin list of workflow types rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="WorkflowTypesAdminListColumnDriver"/>.
/// </summary>
public static class WorkflowTypesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__WorkflowTypes</c> and <c>AdminListCell__WorkflowTypes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "WorkflowTypes";
}


/// <summary>
/// The admin list of workflow instances rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="WorkflowInstancesAdminListColumnDriver"/>.
/// </summary>
public static class WorkflowInstancesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__WorkflowInstances</c> and <c>AdminListCell__WorkflowInstances__{Column}</c> alternates.
    /// </summary>
    public const string Name = "WorkflowInstances";
}
