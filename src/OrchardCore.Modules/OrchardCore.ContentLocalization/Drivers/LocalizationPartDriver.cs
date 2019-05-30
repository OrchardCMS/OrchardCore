using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Drivers
{
    public class LocalizationPartDisplayDriver : ContentPartDisplayDriver<LocalizationPart>
    {
        private readonly IContentLocalizationManager _contentLocalizationManager;
        private readonly ISiteService _siteService;
        private readonly IIdGenerator _iidGenerator;

        public LocalizationPartDisplayDriver(
            IContentLocalizationManager contentLocalizationManager,
            ISiteService siteService,
            IIdGenerator iidGenerator
        )
        {
            _contentLocalizationManager = contentLocalizationManager;
            _siteService = siteService;
            _iidGenerator = iidGenerator;
        }

        public override IDisplayResult Display(LocalizationPart part, BuildPartDisplayContext context)
        {
            return Combine(
                Initialize<LocalizationPartViewModel>("LocalizationPart_SummaryAdmin", model => BuildViewModelAsync(model, part)).Location("SummaryAdmin", "Meta:11"),
                Initialize<LocalizationPartViewModel>("LocalizationPart_SummaryAdminLinks", model => BuildViewModelAsync(model, part)).Location("SummaryAdmin", "Actions:5")
            );
        }

        public override IDisplayResult Edit(LocalizationPart localizationPart)
        {
            return Initialize<LocalizationPartViewModel>("LocalizationPart_Edit", m => BuildViewModelAsync(m, localizationPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(LocalizationPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new LocalizationPartViewModel();
            await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Culture);
            model.Culture = viewModel.Culture;
            // Need to do this here to support displaying the message to save before localizing when the item has not been saved yet.
            if (String.IsNullOrEmpty(model.LocalizationSet))
            {
                model.LocalizationSet = _iidGenerator.GenerateUniqueId();
            }
            return Edit(model, context);
        }

        public async Task BuildViewModelAsync(LocalizationPartViewModel model, LocalizationPart localizationPart)
        {
            var settings = await _siteService.GetSiteSettingsAsync();
            var alreadyTranslated = await _contentLocalizationManager.GetItemsForSet(localizationPart.LocalizationSet);

            model.Culture = localizationPart.Culture;
            model.LocalizationSet = localizationPart.LocalizationSet;
            model.LocalizationPart = localizationPart;

            if (String.IsNullOrEmpty(model.Culture))
            {
                model.Culture = await GetDefaultCultureNameAsync();
            }

            var currentCultures = settings.GetConfiguredCultures().Where(c=>c != model.Culture).Select(culture =>
            {
                return new LocalizationLinksViewModel()
                {
                    IsDeleted = false,
                    Culture = CultureInfo.GetCultureInfo(culture),
                    ContentItemId = alreadyTranslated.FirstOrDefault(c => c.As<LocalizationPart>()?.Culture == culture)?.ContentItemId,
                };
            }).ToList();

            // Content items that have been translated but the culture was removed from the settings page
            var deletedCultureTranslations = alreadyTranslated.Where(c => c.As<LocalizationPart>()?.Culture != model.Culture).Select(ci =>
            {
                var culture = ci.As<LocalizationPart>()?.Culture;
                if (currentCultures.Any(c => c.ContentItemId == ci.ContentItemId) || culture == null)
                {
                    return null;
                }
                return new LocalizationLinksViewModel()
                {
                    IsDeleted = true,
                    Culture = CultureInfo.GetCultureInfo(culture),
                    ContentItemId = ci?.ContentItemId
                };
            }).OfType<LocalizationLinksViewModel>().ToList();

            model.ContentItemCultures = currentCultures.Concat(deletedCultureTranslations).ToList();
        }

        private async Task<string> GetDefaultCultureNameAsync()
        {
            var setting = await _siteService.GetSiteSettingsAsync();

            if (!String.IsNullOrEmpty(setting.Culture))
            {
                return CultureInfo.GetCultureInfo(setting.Culture).Name;
            }

            return CultureInfo.InstalledUICulture.Name;
        }
    }
}