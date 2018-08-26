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

            if (part == null)
            {
                return query;
            }

            var aliasQuery = query.With<AliasPartIndex>();

            if (!string.IsNullOrWhiteSpace(part.Alias))
            {
                return aliasQuery.Where(index => index.Alias == part.Alias);
            }

            return query;
        }
    }
}
