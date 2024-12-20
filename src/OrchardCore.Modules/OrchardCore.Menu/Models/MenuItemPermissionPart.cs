using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models;

public class MenuItemPermissionPart : ContentPart
{
    /// <summary>
    /// The names of the permissions required to view this admin menu node.
    /// </summary>
    public string[] PermissionNames { get; set; } = [];
}
