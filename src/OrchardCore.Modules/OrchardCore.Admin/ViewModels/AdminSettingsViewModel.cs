using OrchardCore.Admin.Models;

namespace OrchardCore.Admin.ViewModels;

public class AdminSettingsViewModel
{
    public bool DisplayThemeToggler { get; set; }

    public bool DisplayMenuFilter { get; set; }

    public bool DisplayQuickNavigation { get; set; }

    public bool DisplayNewMenu { get; set; }

    public bool DisplayTitlesInTopbar { get; set; }

    public bool ShowBreadcrumb { get; set; }

    public AdminMenuBehavior MenuBehavior { get; set; }

    public string ListLayout { get; set; }

    public string ListActionsLayout { get; set; }

    public bool AllowUserListLayoutSelection { get; set; }
}
