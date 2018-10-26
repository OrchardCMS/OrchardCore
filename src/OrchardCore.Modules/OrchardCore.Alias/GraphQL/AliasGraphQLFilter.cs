using GraphQL.Types;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Alias.GraphQL
{
    public class AliasGraphQLFilter : GraphQLFilter<ContentItem>
    {
        public override IQuery<ContentItem> PreQuery(IQuery<ContentItem> query, ResolveFieldContext context)
        {
            if (!context.HasArgument("aliasPart"))
            {
                return query;
            }

            var part = context.GetArgument<AliasPart>("aliasPart");

            if (!string.IsNullOrWhiteSpace(part?.Alias))
            {
                return query.With<AliasPartIndex>(index => index.Alias == part.Alias);
            }

            return query;
        }
    }
}
