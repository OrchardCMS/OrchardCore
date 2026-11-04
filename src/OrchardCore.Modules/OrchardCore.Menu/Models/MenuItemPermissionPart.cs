using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models;

public class MenuItemPermissionPart : ContentPart
{
    /// <summary>
    /// The names of the permissions required to view the menu item.
    /// </summary>
    [Description("The permission names of which the current user must have at least one to view the menu item.")]
    public string[] PermissionNames { get; set; } = [];
}
