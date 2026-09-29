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
    /// Gets or sets how the sections of the admin menu open and close.
    /// </summary>
    [DefaultValue(AdminMenuBehavior.Persistent)]
    public AdminMenuBehavior MenuBehavior { get; set; } = AdminMenuBehavior.Persistent;
}
