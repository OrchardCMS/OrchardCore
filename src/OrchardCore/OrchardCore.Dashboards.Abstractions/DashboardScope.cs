namespace OrchardCore.Dashboards;

/// <summary>
/// Defines who the widgets of a dashboard type belong to.
/// </summary>
public enum DashboardScope
{
    /// <summary>
    /// The dashboard has one set of widgets, shared by every user who can view it.
    /// Users with the <see cref="DashboardTypeDescriptor.ManagePermission"/> arrange it for everyone.
    /// </summary>
    Site,

    /// <summary>
    /// Every user has their own set of widgets on the dashboard, which only they see and arrange.
    /// A user needs the <see cref="DashboardTypeDescriptor.ManagePermission"/> to have one.
    /// </summary>
    User,
}
