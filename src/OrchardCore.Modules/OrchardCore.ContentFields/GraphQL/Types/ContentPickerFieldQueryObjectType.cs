using System.Collections.Generic;
using System.Linq;
using GraphQL.DataLoader;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentFields.GraphQL
{
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
                .ResolveLockedAsync(async x =>
                {
                    var contentItemLoader = x.GetOrAddPublishedContentItemByIdDataLoader();

                    var data = await contentItemLoader.LoadAsync(x.Page(x.Source.ContentItemIds)).GetResultAsync();

                    return data.SelectMany(x => x);
                });
        }
    }
}
