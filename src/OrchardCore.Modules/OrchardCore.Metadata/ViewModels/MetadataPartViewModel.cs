using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Metadata.Models;
using OrchardCore.Metadata.Settings;

namespace OrchardCore.Metadata.ViewModels
{
    public class MetadataPartViewModel
    {
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string OpenGraphTitle { get; set; }
        public string OpenGraphDescription { get; set; }
        public string OpenGraphImage { get; set; }
        /// <summary>
        /// Also used to generate twitter:image:alt
        /// </summary>
        public string OpenGraphImageAlt { get; set; }
        public string OpenGraphVideo { get; set; }
        public string OpenGraphUrl { get; set; }
        public string OpenGraphType { get; set; }
        public string TwitterCard { get; set; }
        public string OpenGraphSite_name { get; set; }
        public string FacebookAppId { get; set; }
        public string TwitterSite { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public MetadataPart MetadataPart { get; set; }

        [BindNever]
        public MetadataPartSettings Settings { get; set; }
    }
}
