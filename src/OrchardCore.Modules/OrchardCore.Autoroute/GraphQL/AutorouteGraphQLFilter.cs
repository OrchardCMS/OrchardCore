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
            if (!context.HasArgument("autoroutePart"))
            {
                return query;
            }

            var part = context.GetArgument<AutoroutePart>("autoroutePart");

            if (!string.IsNullOrWhiteSpace(part?.Path))
            {
                return query.With<AutoroutePartIndex>(index => index.Path == part.Path);
            }
            return query;
        }
    }
}
