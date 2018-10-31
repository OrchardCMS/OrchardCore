using System.Collections.Generic;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
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
            JObject whereArguments)
        {
            if (!whereArguments.TryGetValue("containedPart", out var part))
            {
                return query;
            }

            var containedPart = part.ToObject<ContainedPart>();

            if (!string.IsNullOrWhiteSpace(containedPart?.ListContentItemId))
            {
                return query.With<ContainedPartIndex>(index => index.ListContentItemId == containedPart.ListContentItemId);
            }

            return query;
        }
    }
}
