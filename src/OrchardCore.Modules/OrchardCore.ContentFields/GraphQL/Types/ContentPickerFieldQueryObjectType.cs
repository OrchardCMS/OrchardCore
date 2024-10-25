using GraphQL.DataLoader;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentFields.GraphQL;

public class ContentPickerFieldQueryObjectType : ObjectGraphType<ContentPickerField>
{
    public ContentPickerFieldQueryObjectType()
    {
        Name = nameof(ContentPickerField);

        Field<ListGraphType<StringGraphType>, IEnumerable<string>>("contentItemIds")
            .Description("content item ids")
            .PagingArguments()
            .Resolve(x =>
            {
                return x.Page(x.Source.ContentItemIds);
            });

        Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>("contentItems")
            .Description("the content items")
            .PagingArguments()
            .ResolveAsync(x =>
            {
                var contentItemLoader = x.GetOrAddPublishedContentItemByIdDataLoader();

                return (contentItemLoader.LoadAsync(x.Page(x.Source.ContentItemIds))).Then(itemResultSet =>
                {
                    return itemResultSet.SelectMany(x => x);
                });
            });
    }
}
