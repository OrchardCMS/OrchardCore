using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.GraphQL;

public class MenuItemsListQueryObjectType : ObjectGraphType<MenuItemsListPart>
{
    public MenuItemsListQueryObjectType(IStringLocalizer<MenuItemsListQueryObjectType> S)
    {
        Name = "MenuItemsListPart";

        Field<ListGraphType<MenuItemInterface>>("menuItems")
            .Description(S["The menu items."])
            .Resolve(context => context.Source.MenuItems);
    }
}
