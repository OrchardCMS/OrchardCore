using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql;

namespace OrchardCore.Lists.GraphQL
{
    public class ListQueryObjectType : ObjectGraphType<ListPart>
    {
        public ListQueryObjectType(IStringLocalizer<ListQueryObjectType> T)
        {
            Name = "ListPart";
            Description = T["Represents a collection of content items."];

            Field<ListGraphType<ContentItemInterface>, IEnumerable<ContentItem>>()
                .Name("contentItems")
                .Description("the content items")
                .Argument<IntGraphType, int>("first", "the first n elements (10 by default)", 0)
                .Argument<IntGraphType, int>("skip", "the number of elements to skip", 0)
                .ResolveAsync(async g =>
                {
                    var context = (GraphQLContext)g.UserContext;
                    var session = context.ServiceProvider.GetService<ISession>();

                    var query = session.Query<ContentItem>()
                        .With<ContainedPartIndex>(x => x.ListContentItemId == g.Source.ContentItem.ContentItemId)
                        .With<ContentItemIndex>(x => x.Published)
                        .OrderByDescending(x => x.CreatedUtc);

                    // Apply a default limit
                    var pagedQuery = query.Take(10);

                    var skip = g.GetArgument<int>("skip");
                    var first = g.GetArgument<int>("first");

                    if (skip > 0)
                    {
                        pagedQuery = pagedQuery.Skip(skip);
                    }

                    if (first > 0)
                    {
                        pagedQuery = pagedQuery.Take(first);
                    }

                    return await query.ListAsync();
                });
        }
    }
}
