using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.ContentLocalization.Drivers
{
    public class LocalizationPartDisplayDriver : ContentPartDisplayDriver<LocalizationPart>
    {
        private readonly IContentLocalizationManager _contentLocalizationManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ISession _session;
        private readonly IStringLocalizer<LocalizationPartDisplayDriver> T;

        public LocalizationPartDisplayDriver(
            IContentLocalizationManager contentLocalizationManager,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            ISession session,
            IStringLocalizer<LocalizationPartDisplayDriver> localizer)
        {
            _contentLocalizationManager = contentLocalizationManager;
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _session = session;
            T = localizer;
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

            return Edit(model, context);
        }


        private async Task BuildViewModelAsync(LocalizationPartViewModel model, LocalizationPart localizationPart)
        {
            var settings = await _siteService.GetSiteSettingsAsync();
            var alreadyTranslated = await _contentLocalizationManager.GetItemsForSet(localizationPart.LocalizationSet);

            model.Culture = localizationPart.Culture;
            model.LocalizationSet = localizationPart.LocalizationSet;
            model.LocalizationPart = localizationPart;


            // todo: cleanup this code

            var currentCultures = settings.SupportedCultures.Select(culture =>
            {
                return new LocalizationLinksViewModel()
                {
                    IsDeleted = false,
                    Culture = CultureInfo.GetCultureInfo(culture),
                    ContentItem = alreadyTranslated.FirstOrDefault(c => c.As<LocalizationPart>()?.Culture == culture),
                };
            }).ToList();

            var deletedCultureTranslations = alreadyTranslated.Select(ci =>
              {
                  var culture = ci.As<LocalizationPart>()?.Culture;
                  if (currentCultures.Any(c=>c.ContentItem?.ContentItemId == ci.ContentItemId))
                  {
                      return null;
                  }
                  return new LocalizationLinksViewModel()
                  {
                      IsDeleted = true,
                      Culture = CultureInfo.GetCultureInfo(ci.As<LocalizationPart>()?.Culture),
                      ContentItem = ci
                  };
              }
            ).OfType<LocalizationLinksViewModel>().ToList();

            model.SiteCultures = currentCultures.Concat(deletedCultureTranslations).ToList();
        }
    }
}
