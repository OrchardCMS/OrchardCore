using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Body.Model;
using OrchardCore.ContentManagement;

namespace OrchardCore.Body.GraphQL
{
    public class BodyGraphQLFilter : GraphQLFilter<ContentItem>
    {
        public override IEnumerable<ContentItem> PostQuery(IEnumerable<ContentItem> contentItems, ResolveFieldContext context)
        {
            if (!context.HasArgument("BodyPart"))
            {
                return contentItems;
            }

            var part = context.GetArgument<BodyPart>("BodyPart");

            if (part == null)
            {
                return contentItems;
            }

            if (!string.IsNullOrWhiteSpace(part.Body))
            {
                return contentItems.Where(ci => ci.As<BodyPart>().Body == part.Body);
            }

            return contentItems;
        }
    }
}
