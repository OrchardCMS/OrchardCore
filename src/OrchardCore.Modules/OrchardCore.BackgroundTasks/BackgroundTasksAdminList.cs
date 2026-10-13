namespace OrchardCore.BackgroundTasks;

/// <summary>
/// The admin list of background tasks rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="BackgroundTasksAdminListColumnDriver"/>.
/// </summary>
public static class BackgroundTasksAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__BackgroundTasks</c> and <c>AdminListCell__BackgroundTasks__{Column}</c> alternates.
    /// </summary>
    public const string Name = "BackgroundTasks";
}
