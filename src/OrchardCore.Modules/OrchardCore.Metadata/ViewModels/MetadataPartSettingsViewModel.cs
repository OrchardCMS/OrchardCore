using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Metadata.Models;

namespace OrchardCore.Metadata.ViewModels
{
    public class MetadataPartSettingsViewModel
    {
        public bool SupportOpenGraph { get; set; }
        public bool SupportTwitterCards { get; set; }
        public bool SupportMetaKeywords { get; set; }

        [BindNever]
        public SocialMetadataPartSettings MetadataPartSettings { get; set; }
    }
}
