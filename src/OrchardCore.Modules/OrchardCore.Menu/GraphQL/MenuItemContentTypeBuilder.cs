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

        contentItemType
            .Field<MenuItemsListQueryObjectType>(nameof(MenuItemsListPart).ToFieldName())
            .Resolve(context => context.Source.As<MenuItemsListPart>());

        contentItemType.Interface<MenuItemInterface>();
    }

    public void Clear()
    {
    }
}
