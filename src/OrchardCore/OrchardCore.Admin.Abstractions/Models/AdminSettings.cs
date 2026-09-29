using System.ComponentModel;

namespace OrchardCore.Admin.Models;

public class AdminSettings
{
    [DefaultValue(true)]
    public bool DisplayThemeToggler { get; set; } = true;

    public bool DisplayMenuFilter { get; set; }

    public bool DisplayNewMenu { get; set; }

    public bool DisplayTitlesInTopbar { get; set; }

    /// <summary>
    /// Gets or sets how the sections of the admin menu open and close.
    /// </summary>
    [DefaultValue(AdminMenuBehavior.Persistent)]
    public AdminMenuBehavior MenuBehavior { get; set; } = AdminMenuBehavior.Persistent;
}
