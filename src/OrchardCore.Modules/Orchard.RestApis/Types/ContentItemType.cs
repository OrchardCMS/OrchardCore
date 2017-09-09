using GraphQL.Types;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;

namespace Orchard.RestApis.Types
{
    public class ContentItemType : ObjectGraphType<ContentItem>
    {
        public ContentItemType(IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager)
        {
            Name = "contentitem";

            Field("id", h => h.ContentItemId).Description("The id of the content item.");

            Field(h => h.Published);
            Field(h => h.Latest);

            Field(h => h.Number);
            Field(h => h.ContentType);
            Field(h => h.ContentItemVersionId);
        }
    }
}
