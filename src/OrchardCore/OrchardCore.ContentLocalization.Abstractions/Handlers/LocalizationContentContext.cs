using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class LocalizationContentContext : ContentContextBase
    {
        public LocalizationContentContext(ContentItem contentItem, ContentItem original, string localizationSet, string culture)
            : base(contentItem)
        {
            Original = original;
            LocalizationSet = localizationSet;
            Culture = culture;
        }

        public ContentItem Original { get; }

        public string LocalizationSet { get; }

        public string Culture { get; }
    }
}
