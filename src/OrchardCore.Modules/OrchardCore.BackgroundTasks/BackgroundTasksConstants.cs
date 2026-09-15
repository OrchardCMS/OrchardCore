namespace OrchardCore.BackgroundTasks;

/// <summary>
/// The names of the breadcrumbs rendered by the background tasks screens. A module adds a node to one of these trails
/// by registering an <c>OrchardCore.Navigation.IBreadcrumbProvider</c> that reacts to one of these names.
/// </summary>
public static class BackgroundTasksConstants
{
    /// <summary>
    /// The breadcrumb of the background tasks list. It is named after the list of that screen.
    /// </summary>
    public const string List = "BackgroundTasks";

    /// <summary>
    /// The breadcrumb of the background task settings screen.
    /// </summary>
    public const string Edit = "BackgroundTasksEdit";
}
