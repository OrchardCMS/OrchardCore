using OrchardCore.ContentManagement;
using OrchardCore.Metadata.Fields;

namespace OrchardCore.Metadata.Models
{
    public class SeoMetadataPart : ContentPart
    {
        public MetadataTextField PageTitle { get; set; }
        public MetadataTextField MetaDescription { get; set; }
        public MetadataTextField MetaKeywords { get; set; }
    }
}
