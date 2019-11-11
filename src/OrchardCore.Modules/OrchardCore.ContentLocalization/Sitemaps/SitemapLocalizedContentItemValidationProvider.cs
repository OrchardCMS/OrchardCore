using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;
using OrchardCore.Sitemaps.Builders;

namespace OrchardCore.ContentLocalization.Sitemaps
{
    public class SitemapLocalizedContentItemValidationProvider : ISitemapContentItemValidationProvider
    {
        private readonly ILocalizationService _localizationService;

        public SitemapLocalizedContentItemValidationProvider(
            ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public async Task<bool> ValidateContentItem(ContentItem contentItem)
        {
            var part = contentItem.As<LocalizationPart>();
            if (part == null)
            {
                return true;
            }

            // Content item is valid if it is for the default culture.
            var defaultCulture = await _localizationService.GetDefaultCultureAsync();
            if (part.Culture == defaultCulture)
            {
                return true;
            }

            return false;
        }
    }
}
