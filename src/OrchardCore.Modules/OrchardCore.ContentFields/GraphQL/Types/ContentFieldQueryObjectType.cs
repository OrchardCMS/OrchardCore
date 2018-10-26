using System.Collections.Generic;
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

            Field("contentItemIds", x => x.ContentItemIds, nullable: true)
                .Description("the content item ids")
                .Type(new StringGraphType())
                ;

            Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>()
                .Name("contentItems")
                .Description("the content items")
                //.Type(new ListGraphType<ContentItemType>())
                .ResolveAsync(x =>
                {
                    var ids = x.Source.ContentItemIds;
                    var context = (GraphQLContext)x.UserContext;
                    var session = context.ServiceProvider.GetService<ISession>();
                    return session.Query<ContentItem, ContentItemIndex>(y => y.ContentItemId.IsIn(ids)).ListAsync();
                });
        }
    }
}
