using System.Collections.Generic;
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
        public override IQuery<ContentItem> PreQuery(IQuery<ContentItem> query, ResolveFieldContext context,
            Dictionary<string, object> whereArguments)
        {
            if (!whereArguments.ContainsKey("containedPart"))
            {
                return query;
            }

            var part = whereArguments["containedPart"] as ContainedPart;

            if (!string.IsNullOrWhiteSpace(part?.ListContentItemId))
            {
                return query.With<ContainedPartIndex>(index => index.ListContentItemId == part.ListContentItemId);
            }

            return query;
        }
    }
}
