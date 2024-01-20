using GraphQL.Resolvers;
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
            AddField(new FieldType()
            {
                Name = "menuItemsList",
                ResolvedType = new MenuItemsListQueryObjectType(),
                Resolver = new FuncFieldResolver<ContentItem, MenuItemsListPart>(context =>
                {
                    return context.Source.As<MenuItemsListPart>();
                })

            });
        }
    }
}
