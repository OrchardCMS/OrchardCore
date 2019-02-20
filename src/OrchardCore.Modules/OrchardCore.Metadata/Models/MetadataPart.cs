using OrchardCore.ContentManagement;

namespace OrchardCore.Metadata.Models
{
    public class MetadataPart : ContentPart
    {
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string OpenGraphTitle { get; set; }
        public string OpenGraphDescription { get; set; }
        public string OpenGraphImage { get; set; }
        public string OpenGraphVideo { get; set; }
        public string OpenGraphImageAlt { get; set; }
        public string OpenGraphType { get; set; }
        public string TwitterCard { get; set; }
    }
}
