using GraphQL.Types;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.GraphQL;

public class MenuItemsListQueryObjectType : ObjectGraphType<MenuItemsListPart>
{
    public MenuItemsListQueryObjectType()
    {
        Name = "MenuItemsListPart";

        Field<ListGraphType<MenuItemInterface>>("menuItems")
            .Description("The menu items.")
            .Resolve(context => context.Source.MenuItems);

    }
}
