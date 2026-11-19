using System.Security.Claims;
using OrchardCore.ContentManagement;

namespace OrchardCore.Dashboards.Services;

/// <summary>
/// Provides the registered dashboard types, the access users have to them, and their widgets.
/// </summary>
public interface IDashboardManager
{
    /// <summary>
    /// Gets the registered dashboard type with the given name.
    /// </summary>
    /// <param name="name">The name of the dashboard type.</param>
    /// <returns>The dashboard type, or <see langword="null"/> when no type is registered with this name.</returns>
    DashboardTypeDescriptor GetType(string name);

    /// <summary>
    /// Gets the registered dashboard types.
    /// </summary>
    IEnumerable<DashboardTypeDescriptor> GetTypes();

    /// <summary>
    /// Checks whether a user can view a dashboard.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="type">The dashboard type.</param>
    Task<bool> CanViewAsync(ClaimsPrincipal user, DashboardTypeDescriptor type);

    /// <summary>
    /// Checks whether a user can add, arrange and remove the widgets of a dashboard.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="type">The dashboard type.</param>
    Task<bool> CanManageAsync(ClaimsPrincipal user, DashboardTypeDescriptor type);

    /// <summary>
    /// Gets the widgets a user sees on a dashboard, ordered by position: every widget of a <see cref="DashboardScope.Site"/>
    /// dashboard, or the user's own widgets of a <see cref="DashboardScope.User"/> dashboard.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="type">The dashboard type.</param>
    /// <param name="latest"><see langword="true"/> to get the latest versions of the widgets, <see langword="false"/> for the published ones.</param>
    Task<IEnumerable<ContentItem>> GetWidgetsAsync(ClaimsPrincipal user, DashboardTypeDescriptor type, bool latest);

    /// <summary>
    /// Checks whether a widget is on the dashboard a user sees.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="type">The dashboard type.</param>
    /// <param name="widget">The widget.</param>
    bool IsOnDashboard(ClaimsPrincipal user, DashboardTypeDescriptor type, ContentItem widget);
}
