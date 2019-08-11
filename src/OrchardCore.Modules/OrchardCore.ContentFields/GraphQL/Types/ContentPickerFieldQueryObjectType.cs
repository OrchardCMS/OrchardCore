using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using YesSql;
using YesSql.Services;

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

            Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>()
                .Name("contentItems")
                .Description("the content items")
                .PagingArguments()
                .ResolveAsync(async x =>
                {
                    var ids = x.Page(x.Source.ContentItemIds);
                    var context = (GraphQLContext)x.UserContext;
                    var session = context.ServiceProvider.GetService<ISession>();
                    var contentItems = await session.Query<ContentItem, ContentItemIndex>(y => y.ContentItemId.IsIn(ids) && y.Published).ListAsync();
                    return contentItems.OrderBy(c => Array.IndexOf(x.Source.ContentItemIds, c.ContentItemId));
                });
        }
    }
}
