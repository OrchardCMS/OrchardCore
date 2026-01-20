using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization.Models
{
    public class LocalizationPart : ContentPart, ILocalizable
    {
        public string LocalizationSet { get; set; }
        public string Culture { get; set; }
    }
}
