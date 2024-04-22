using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ActivatingContentContext : ContentContextBase
    {
        public ActivatingContentContext(ContentItem contentItem) : base(contentItem)
        {
        }

        public string ContentType { get; set; }
        public ContentTypeDefinition Definition { get; set; }
    }
}
