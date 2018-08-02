using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql;

namespace OrchardCore.Lists.GraphQL
{
    public class ContainedGraphQLFilter : GraphQLFilter<ContentItem>
    {
        public override IQuery<ContentItem> PreQuery(IQuery<ContentItem> query, ResolveFieldContext context)
        {
            if (!context.HasArgument("ContainedPart"))
            {
                return query;
            }

            var part = context.GetArgument<ContainedPart>("ContainedPart");

            if (part == null)
            {
                return query;
            }

            var containedQuery = query.With<ContainedPartIndex>();

            if (!string.IsNullOrWhiteSpace(part.ListContentItemId))
            {
                return containedQuery.Where(index => index.ListContentItemId == part.ListContentItemId);
            }

            return query;
        }
    }
}
