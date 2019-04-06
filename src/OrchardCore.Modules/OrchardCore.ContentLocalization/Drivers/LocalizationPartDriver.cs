using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.ViewModels;
using YesSql;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using System.Linq;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Settings;
using System.Collections.Generic;
using System.Globalization;
using OrchardCore.ContentManagement;

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

        //public override IDisplayResult Display(LocalizationPart localizationPart, BuildPartDisplayContext context)
        //{
        //    return Initialize<LocalizationPartViewModel>("LocalizationPart", m => BuildViewModel(m, localizationPart))
        //        .Location("Detail", "Content:5")
        //        .Location("Summary", "Content:10");
        //}


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
            model.SiteCultures = settings.SupportedCultures.Select(culture =>
            {
                return new LocalizationLinksViewModel() {
                    Culture = CultureInfo.GetCultureInfo(culture),
                    // This code feels wrong
                    ContentItemId = alreadyTranslated.FirstOrDefault(c=>c.As<LocalizationPart>()?.Culture == culture)?.ContentItemId };
            });
        }
    }
}
