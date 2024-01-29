using System.ComponentModel;

namespace OrchardCore.Admin.Models;

public class AdminSettings
{
    public const string AdminMenuId = "adminMenu";

    [DefaultValue(true)]
    public bool DisplayThemeToggler { get; set; } = true;

    public bool DisplayMenuFilter { get; set; }

    public bool DisplayNewMenu { get; set; }

    public bool DisplayTitlesInTopbar { get; set; }
}
