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
        public override IQuery<ContentItem> PreQuery(IQuery<ContentItem> query, ResolveFieldContext context)
        {
            if (!context.HasArgument("AutoroutePart"))
            {
                return query;
            }

            var part = context.GetArgument<AutoroutePart>("AutoroutePart");

            if (part == null)
            {
                return query;
            }

            var autorouteQuery = query.With<AutoroutePartIndex>();

            if (!string.IsNullOrWhiteSpace(part.Path))
            {
                return autorouteQuery.Where(index => index.Path == part.Path);
            }
            return query;
        }
    }
}
