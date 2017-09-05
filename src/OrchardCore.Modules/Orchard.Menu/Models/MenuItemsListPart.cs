using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models
{
    // A content item with this part can have menu items.
    // This part is automatically added to all menus.
    public class MenuItemsListPart : ContentPart
    {
        public List<ContentItem> MenuItems { get; set; } = new List<ContentItem>();
    }
}
