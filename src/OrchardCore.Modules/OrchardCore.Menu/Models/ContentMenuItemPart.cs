using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models;

public class ContentMenuItemPart : ContentPart
{
    [Description("Whether the current user must be authorized to view the linked content item.")]
    public bool CheckContentPermissions { get; set; }
}
