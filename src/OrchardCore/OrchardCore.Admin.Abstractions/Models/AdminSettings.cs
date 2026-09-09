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
    /// The default layout used to render admin lists, e.g. <see cref="AdminListLayouts.List"/> or <see cref="AdminListLayouts.Table"/>.
    /// </summary>
    [DefaultValue(AdminListLayouts.List)]
    public string ListLayout { get; set; } = AdminListLayouts.List;

    /// <summary>
    /// The default layout of the row actions of admin lists, e.g. <see cref="AdminListActionsLayouts.Buttons"/> or <see cref="AdminListActionsLayouts.Menu"/>.
    /// </summary>
    [DefaultValue(AdminListActionsLayouts.Buttons)]
    public string ListActionsLayout { get; set; } = AdminListActionsLayouts.Buttons;
}
