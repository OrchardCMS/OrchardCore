using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class ContentFieldType : ObjectGraphType<ContentField>
    {
        public ContentFieldType()
        {
            Name = "ContentField";
        }
    }
}
