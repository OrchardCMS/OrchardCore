namespace OrchardCore.Admin;

/// <summary>
/// The defaults used by admin lists when the site settings do not specify a value.
/// </summary>
/// <remarks>
/// Configurable per tenant from <c>appsettings.json</c>:
/// <code>
/// {
///   "OrchardCore": {
///     "AdminList": {
///       "DefaultLayout": "Table",
///       "DefaultActionsLayout": "Menu",
///       "AllowUserSelection": true
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
    /// The layout used to render admin lists, e.g. <see cref="AdminListConstants.List"/>,
    /// <see cref="AdminListConstants.Table"/> or <see cref="AdminListConstants.Grid"/>.
    /// </summary>
    public string DefaultLayout { get; set; } = AdminListConstants.List;

    /// <summary>
    /// The layout used to render the actions of each row, e.g. <see cref="AdminListActionsLayouts.Buttons"/>
    /// or <see cref="AdminListActionsLayouts.Menu"/>.
    /// </summary>
    public string DefaultActionsLayout { get; set; } = AdminListActionsLayouts.Buttons;

    /// <summary>
    /// Whether a list shows a selector letting the user render it with another layout. The choice is kept in a
    /// cookie, per list, and only applies to that user.
    /// </summary>
    public bool AllowUserSelection { get; set; }
}
