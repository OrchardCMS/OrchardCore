using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization.Models
{
    public class LocalizationPart : ContentPart
    {
        public string LocalizationSet { get; set; }

        public string Culture { get; set; }
    }
}
