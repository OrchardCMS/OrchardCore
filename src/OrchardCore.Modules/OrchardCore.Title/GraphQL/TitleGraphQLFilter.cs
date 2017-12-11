using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.GraphQL
{
    public class TitleGraphQLFilter : GraphQLFilter<ContentItem>
    {
        public override IEnumerable<ContentItem> PostQuery(IEnumerable<ContentItem> contentItems, ResolveFieldContext context)
        {
            if (!context.HasArgument("TitlePart"))
            {
                return contentItems;
            }

            var part = context.GetArgument<TitlePart>("TitlePart");

            if (part == null)
            {
                return contentItems;
            }

            if (!string.IsNullOrWhiteSpace(part.Title))
            {
                return contentItems.Where(ci => ci.As<TitlePart>().Title == part.Title);
            }

            return contentItems;
        }
    }
}
