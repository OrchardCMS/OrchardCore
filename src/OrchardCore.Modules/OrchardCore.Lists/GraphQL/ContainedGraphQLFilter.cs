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
            if (!context.HasArgument("containedPart"))
            {
                return query;
            }

            var part = context.GetArgument<ContainedPart>("containedPart");

            if (!string.IsNullOrWhiteSpace(part?.ListContentItemId))
            {
                return query.With<ContainedPartIndex>(index => index.ListContentItemId == part.ListContentItemId);
            }

            return query;
        }
    }
}
