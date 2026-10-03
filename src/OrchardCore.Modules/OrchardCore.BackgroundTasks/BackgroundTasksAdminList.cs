namespace OrchardCore.BackgroundTasks;

/// <summary>
/// The admin list of background tasks rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="BackgroundTasksAdminListColumnProvider"/>.
/// </summary>
public static class BackgroundTasksAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__BackgroundTasks</c> and <c>AdminListCell__BackgroundTasks__{Column}</c> alternates.
    /// </summary>
    public const string Name = "BackgroundTasks";
}
