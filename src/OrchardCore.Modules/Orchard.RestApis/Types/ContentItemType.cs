using GraphQL.Types;
using Orchard.ContentManagement;

namespace Orchard.RestApis.Types
{
    public class ContentItemType : ObjectGraphType<ContentItem>
    {
        public ContentItemType(IContentManager contentManager)
        {
            Name = "contentitem";

            Field(h => h.Id).Description("The id of the content item.");
            Field(h => h.ContentType).Description("The content type.");
        }
    }
}
