using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Dashboards;

/// <summary>
/// Describes a type of dashboard: a page made of widgets, for example the admin dashboard.
/// </summary>
/// <remarks>
/// A descriptor is shared by every request of the tenant: it has no dependencies and keeps no state.
/// </remarks>
public sealed class DashboardTypeDescriptor
{
    /// <summary>
    /// Creates a new instance of <see cref="DashboardTypeDescriptor"/>.
    /// </summary>
    /// <param name="name">The technical name of the type, stored on its widgets and used in its URLs.</param>
    /// <param name="displayName">The name shown to users, as the title of the dashboard.</param>
    public DashboardTypeDescriptor(string name, LocalizationSource displayName)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(displayName);

        Name = name;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the technical name of the type, stored on its widgets and used in its URLs.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the name shown to users, as the title of the dashboard.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets or sets who the widgets of the dashboard belong to. Defaults to <see cref="DashboardScope.Site"/>.
    /// </summary>
    public DashboardScope Scope { get; set; } = DashboardScope.Site;

    /// <summary>
    /// Gets or sets the permission required to view a <see cref="DashboardScope.Site"/> dashboard.
    /// When not set, the <see cref="ManagePermission"/> is required. It isn't used by a
    /// <see cref="DashboardScope.User"/> dashboard, where users view the widgets they manage.
    /// </summary>
    public Permission ViewPermission { get; set; }

    /// <summary>
    /// Gets or sets the permission required to add, arrange and remove the widgets of the dashboard:
    /// the shared widgets of a <see cref="DashboardScope.Site"/> dashboard, or the user's own widgets of a
    /// <see cref="DashboardScope.User"/> dashboard. When not set, nobody can manage the dashboard.
    /// </summary>
    public Permission ManagePermission { get; set; }
}
