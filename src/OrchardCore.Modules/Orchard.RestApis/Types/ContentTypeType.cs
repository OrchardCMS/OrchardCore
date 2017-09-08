using GraphQL.Types;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.RestApis.Types
{
    public class ContentTypeType : ObjectGraphType<ContentTypeDefinition>
    {
        public ContentTypeType(IContentManager contentManager)
        {
            Name = "contenttype";

            Field(h => h.Name).Description("The content type.");
        }
    }
}
