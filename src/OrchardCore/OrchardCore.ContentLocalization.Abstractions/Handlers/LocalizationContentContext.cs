using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class LocalizationContentContext : ContentContextBase
    {
        public ContentItem Original { get; set; }
        public string LocalizationSet { get; set; }
        public string Culture { get; set; }
        public LocalizationContentContext(ContentItem contentItem, ContentItem original, string localizationSet, string culture)
            : base(contentItem)
        {
            Original = original;
            LocalizationSet = localizationSet;
            Culture = culture;
        }
    }
}
