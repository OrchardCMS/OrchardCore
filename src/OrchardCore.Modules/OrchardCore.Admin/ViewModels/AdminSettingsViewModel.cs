namespace OrchardCore.Admin.ViewModels;

public class AdminSettingsViewModel
{
    public bool DisplayThemeToggler { get; set; }

    public bool DisplayMenuFilter { get; set; }

    public bool DisplayNewMenu { get; set; }

    public bool DisplayTitlesInTopbar { get; set; }

    public string ListLayout { get; set; }

    public string ListActionsLayout { get; set; }

    public bool AllowUserListLayoutSelection { get; set; }
}
