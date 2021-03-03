using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class ContentTypesContext
    {
        public ContentTypeDefinition ContentTypeDefinition { get; }
        public bool IsIgnored { get; set; }

        public ContentTypesContext(ContentTypeDefinition contentTypeDefinition)
        {
            ContentTypeDefinition = contentTypeDefinition;
        }
    }
}
