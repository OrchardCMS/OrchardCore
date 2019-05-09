using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class LocalizationContentContext : ContentContextBase
    {
        public string LocalizationSet { get; set; }
        public string Culture { get; set; }
        public LocalizationContentContext(ContentItem contentItem, string localizationSet, string culture)
            : base(contentItem)
        {
            LocalizationSet = localizationSet;
            Culture = culture;
        }
    }
}
