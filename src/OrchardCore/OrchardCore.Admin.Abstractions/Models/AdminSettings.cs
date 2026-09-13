using System.ComponentModel;

namespace OrchardCore.Admin.Models;

public class AdminSettings
{
    [DefaultValue(true)]
    public bool DisplayThemeToggler { get; set; } = true;

    public bool DisplayMenuFilter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the quick search command palette (Ctrl+K / Cmd+K) is displayed in the admin navbar.
    /// </summary>
    [DefaultValue(true)]
    public bool DisplayQuickSearch { get; set; } = true;

    public bool DisplayNewMenu { get; set; }

    public bool DisplayTitlesInTopbar { get; set; }
}
