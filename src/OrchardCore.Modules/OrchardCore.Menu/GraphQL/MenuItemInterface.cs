using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.GraphQL
{
    public class MenuItemInterface : InterfaceGraphType<ContentItem>
    {
        public MenuItemInterface()
        {
            Name = "MenuItem";

            Field(typeof(MenuItemsListQueryObjectType), "menuItemsList", resolve: context => context.Source.As<MenuItemsListPart>());
        }
    }
}
