namespace OrchardCore.Admin;

/// <summary>
/// The defaults used by admin lists when the site settings do not specify a value.
/// </summary>
/// <remarks>
/// Configurable per tenant from <c>appsettings.json</c>, in the <c>List</c> section of the admin:
/// <code>
/// {
///   "OrchardCore": {
///     "Admin": {
///       "List": {
///         "DefaultLayout": "Grid",
///         "DefaultActionsLayout": "Menu",
///         "AllowUserSelection": true
///       }
///     }
///   }
/// }
/// </code>
/// An administrator can still override these from <b>Configuration → Settings → Admin</b>, which stores the
/// choice in the site settings. Clearing that choice falls back to these values.
/// </remarks>
public class AdminListOptions
{
    /// <summary>
    /// The layout used to render admin lists, e.g. <see cref="AdminListLayout.List"/> or
    /// <see cref="AdminListLayout.Grid"/>.
    /// </summary>
    public AdminListLayout DefaultLayout { get; set; } = AdminListLayout.List;

    /// <summary>
    /// The layout used to render the actions of each row, either <see cref="AdminListActionsLayout.Buttons"/>
    /// or <see cref="AdminListActionsLayout.Menu"/>.
    /// </summary>
    public AdminListActionsLayout DefaultActionsLayout { get; set; } = AdminListActionsLayout.Buttons;

    /// <summary>
    /// Whether a list shows a selector letting the user render it with another layout. The choice is kept in a
    /// cookie, per list, and only applies to that user.
    /// </summary>
    public bool AllowUserSelection { get; set; }
}
