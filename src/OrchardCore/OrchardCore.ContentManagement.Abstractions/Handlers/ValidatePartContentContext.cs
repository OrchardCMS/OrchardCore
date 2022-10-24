using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ValidatePartContentContext : ValidateContentContext
    {
        public ValidatePartContentContext(ContentItem contentItem, ContentTypePartDefinition contentTypePartDefinition) : base(contentItem)
        {
            ContentTypePartDefinition = contentTypePartDefinition;
        }

        public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
    }
}
