using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class ContentPartInputObjectType : ObjectGraphType<ContentPart>
    {
        public ContentPartInputObjectType()
        {
            Name = "ContentPart";

            Field(x => x.ContentItem);
        }
    }
}
