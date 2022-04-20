using System.Collections.Generic;
using System.Linq;
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

            Field<ListGraphType<StringGraphType>, IEnumerable<string>>()
                .Name("contentItemIds")
                .Description("content item ids")
                .PagingArguments()
                .Resolve(x =>
                {
                    return x.Page(x.Source.ContentItemIds);
                });

            Field<ListGraphType<ContentItemInterface>, ContentItem[]>()
                .Name("contentItems")
                .Description("the content items")
                .PagingArguments()
                .ResolveAsync(async x =>
                {
                    var contentItemLoader = x.GetOrAddPublishedContentItemByIdDataLoader();
                    var items = await contentItemLoader.LoadAsync(x.Page(x.Source.ContentItemIds));
                    return items.Where(item => item != null).ToArray();
                });
        }
    }
}
