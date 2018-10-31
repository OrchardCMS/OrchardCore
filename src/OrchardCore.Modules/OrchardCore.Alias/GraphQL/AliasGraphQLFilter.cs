using System.Collections.Generic;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasGraphQLFilter : GraphQLFilter<ContentItem>
    {
        public override IQuery<ContentItem> PreQuery(IQuery<ContentItem> query, ResolveFieldContext context,
            JObject whereArguments)
        {
            if (!whereArguments.TryGetValue("aliasPart", out var part))
            {
                return query;
            }

            var aliasPart = part.ToObject<AliasPart>();

            if (!string.IsNullOrWhiteSpace(aliasPart?.Alias))
            {
                return query.With<AliasPartIndex>(index => index.Alias == aliasPart.Alias);
            }

            return query;
        }
    }
}
