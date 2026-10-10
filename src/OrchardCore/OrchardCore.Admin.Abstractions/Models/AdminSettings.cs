using System.ComponentModel;

namespace OrchardCore.Admin.Models;

public class AdminSettings
{
    [DefaultValue(true)]
    public bool DisplayThemeToggler { get; set; } = true;

    public bool DisplayMenuFilter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the quick navigation palette (Ctrl+K / Cmd+K) is displayed in the admin navbar.
    /// </summary>
    [DefaultValue(true)]
    public bool DisplayQuickNavigation { get; set; } = true;

    public bool DisplayNewMenu { get; set; }

    public bool DisplayTitlesInTopbar { get; set; }

    [DefaultValue(true)]
    public bool ShowBreadcrumb { get; set; } = true;

    /// <summary>
    /// Gets or sets how the sections of the admin menu open and close. Defaults to
    /// <see cref="AdminMenuBehavior.Focused"/>.
    /// </summary>
    public AdminMenuBehavior MenuBehavior { get; set; }

    /// <summary>
    /// The layout used to render admin lists, e.g. <see cref="AdminListConstants.List"/> or <see cref="AdminListConstants.Grid"/>.
    /// When empty, <see cref="AdminListOptions.DefaultLayout"/> is used.
    /// </summary>
    public string ListLayout { get; set; }

    /// <summary>
    /// The layout of the row actions of admin lists, e.g. <see cref="AdminListActionsLayouts.Buttons"/> or <see cref="AdminListActionsLayouts.Menu"/>.
    /// When empty, <see cref="AdminListOptions.DefaultActionsLayout"/> is used.
    /// </summary>
    public string ListActionsLayout { get; set; }

    /// <summary>
    /// Whether a list shows a selector letting the user render it with another layout. The choice is kept in a
    /// cookie, per list, and only applies to that user. When this is off, every list uses
    /// <see cref="ListLayout"/>.
    /// </summary>
    public bool AllowUserListLayoutSelection { get; set; }
}
