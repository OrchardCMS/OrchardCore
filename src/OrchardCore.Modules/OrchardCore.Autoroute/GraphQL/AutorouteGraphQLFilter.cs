using System.Collections.Generic;
using GraphQL.Types;
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
            Dictionary<string, object> whereArguments)
        {
            if (!whereArguments.ContainsKey("autoroutePart"))
            {
                return query;
            }

            var part = whereArguments["autoroutePart"] as AutoroutePart;

            if (!string.IsNullOrWhiteSpace(part?.Path))
            {
                return query.With<AutoroutePartIndex>(index => index.Path == part.Path);
            }
            return query;
        }
    }
}
