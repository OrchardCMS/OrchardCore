using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models;

// A content item with this part can have menu items.
// This part is automatically added to all menus.
public class MenuItemsListPart : ContentPart
{
    [Description("The ordered menu item content items contained by this menu.")]
    public List<ContentItem> MenuItems { get; set; } = [];
}
