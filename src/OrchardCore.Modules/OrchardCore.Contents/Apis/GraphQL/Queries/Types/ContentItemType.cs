using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Apis.GraphQL.Queries.Types
{
    public class ContentItemType : AutoRegisteringObjectGraphType<ContentItem>
    {
        public ContentItemType()
        {
            Name = "ContentItem";
        }
    }
}
