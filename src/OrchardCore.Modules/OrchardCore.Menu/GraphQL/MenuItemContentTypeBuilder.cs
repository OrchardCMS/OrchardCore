using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.GraphQL;

public class MenuItemContentTypeBuilder : IContentTypeBuilder
{
    public void Build(ISchema schema, FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
    {
        if (contentTypeDefinition.GetStereotype() != "MenuItem")
        {
            return;
        }

        contentItemType.AddField(new FieldType
        {
            Name = nameof(MenuItemsListPart).ToFieldName(),
            Type = typeof(MenuItemsListQueryObjectType),
            Resolver = new FuncFieldResolver<ContentItem, MenuItemsListPart>(context => context.Source.As<MenuItemsListPart>()),
        });

        contentItemType.Interface<MenuItemInterface>();
    }
}
