using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class ContentPartInputObjectType : ObjectGraphType<ContentPart>
    {
        public ContentPartInputObjectType()
        {
            Name = "ContentPart";
        }
    }
}
