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

            Field(typeof(MenuItemsListQueryObjectType), "menuItemsListPart", resolve: context => context.Source.As<MenuItemsListPart>());
        }
    }
}