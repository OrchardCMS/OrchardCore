using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Apis.GraphQL.Types
{
    public class ContentItemType : AutoRegisteringObjectGraphType<ContentItem>
    {
        public ContentItemType()
        {
            Name = "ContentItem";
        }
    }
}
