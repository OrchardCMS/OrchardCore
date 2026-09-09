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
    /// The layout used to render admin lists, e.g. <see cref="AdminListConstants.List"/> or <see cref="AdminListConstants.Table"/>.
    /// When empty, <see cref="AdminListOptions.DefaultLayout"/> is used.
    /// </summary>
    public string ListLayout { get; set; }

    /// <summary>
    /// The layout of the row actions of admin lists, e.g. <see cref="AdminListActionsLayouts.Buttons"/> or <see cref="AdminListActionsLayouts.Menu"/>.
    /// When empty, <see cref="AdminListOptions.DefaultActionsLayout"/> is used.
    /// </summary>
    public string ListActionsLayout { get; set; }
}
