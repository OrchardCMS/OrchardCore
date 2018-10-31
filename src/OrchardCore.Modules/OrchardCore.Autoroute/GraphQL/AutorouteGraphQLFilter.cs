using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Autoroute.Model;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Autoroute.GraphQL
{
    public class AutorouteGraphQLFilter : GraphQLFilter<ContentItem>
    {
        public override IQuery<ContentItem> PreQuery(IQuery<ContentItem> query, ResolveFieldContext context,
            JObject whereArguments)
        {
            if (!whereArguments.TryGetValue("autoroutePart", out var part))
            {
                return query;
            }

            var autoroutePart = part.ToObject<AutoroutePart>();

            if (!string.IsNullOrWhiteSpace(autoroutePart?.Path))
            {
                return query.With<AutoroutePartIndex>(index => index.Path == autoroutePart.Path);
            }
            return query;
        }
    }
}
