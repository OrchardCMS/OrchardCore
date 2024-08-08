using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.GraphQL;

public class MenuItemInterface : InterfaceGraphType<ContentItem>
{
    public MenuItemInterface()
    {
        Name = "MenuItem";
        Field<MenuItemsListQueryObjectType>("menuItemsList");
    }
}
