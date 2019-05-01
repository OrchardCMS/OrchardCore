using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Services {

    public interface ILocalizationPartViewModelBuilder {
        Task BuildViewModelAsync(LocalizationPartViewModel model, LocalizationPart part);
    }
    public class LocalizationPartViewModelBuilder : ILocalizationPartViewModelBuilder {
        private readonly IContentLocalizationManager _contentLocalizationManager;
        private readonly ISiteService _siteService;
        public LocalizationPartViewModelBuilder(
            IContentLocalizationManager contentLocalizationManager,
            ISiteService siteService
        ) {
            _contentLocalizationManager = contentLocalizationManager;
            _siteService = siteService;
        }
        public async Task BuildViewModelAsync(LocalizationPartViewModel model, LocalizationPart localizationPart) {
            var settings = await _siteService.GetSiteSettingsAsync();
            var alreadyTranslated = await _contentLocalizationManager.GetItemsForSet(localizationPart.LocalizationSet);

            model.Culture = localizationPart.Culture;
            model.LocalizationSet = localizationPart.LocalizationSet;
            model.LocalizationPart = localizationPart;

            var currentCultures = settings.SupportedCultures.Select(culture => {
                return new LocalizationLinksViewModel() {
                IsDeleted = false,
                Culture = CultureInfo.GetCultureInfo(culture),
                ContentItem = alreadyTranslated.FirstOrDefault(c => c.As<LocalizationPart>()?.Culture == culture),
                };
            }).ToList();
            // Content items that have been translated but the culture was removed from the settings page
            var deletedCultureTranslations = alreadyTranslated.Select(ci => {
                var culture = ci.As<LocalizationPart>()?.Culture;
                if (currentCultures.Any(c => c.ContentItem?.ContentItemId == ci.ContentItemId) || culture == null) {
                    return null;
                }
                return new LocalizationLinksViewModel() {
                    IsDeleted = true,
                        Culture = CultureInfo.GetCultureInfo(culture),
                        ContentItem = ci
                };
            }).OfType<LocalizationLinksViewModel>().ToList();

            model.ContentItemCultures = currentCultures.Concat(deletedCultureTranslations).ToList();
        }
    }
}