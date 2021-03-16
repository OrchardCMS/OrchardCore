using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Metadata.Models;
using OrchardCore.Metadata.Settings;

namespace OrchardCore.Metadata.ViewModels
{
    public class MetadataPartViewModel
    { 

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public MetadataPart MetadataPart { get; set; }

        [BindNever]
        public SocialMetadataPartSettings Settings { get; set; }
    }
}
