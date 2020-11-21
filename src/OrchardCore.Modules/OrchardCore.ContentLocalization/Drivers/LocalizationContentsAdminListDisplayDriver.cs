using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.ContentLocalization.Drivers
{
    public class LocalizationContentsAdminListDisplayDriver : DisplayDriver<ContentOptionsViewModel>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IStringLocalizer S;

        public LocalizationContentsAdminListDisplayDriver(
            ILocalizationService localizationService,
            IStringLocalizer<LocalizationContentsAdminListDisplayDriver> stringLocalizer)
        {
            _localizationService = localizationService;
            S = stringLocalizer;
        }

        protected override void BuildPrefix(ContentOptionsViewModel model, string htmlFieldPrefix)
        {
            Prefix = "Localization";
        }

        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Initialize<LocalizationContentsAdminFilterViewModel>("ContentsAdminList__LocalizationPartFilter", async m =>
                {
                    var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
                    var cultures = new List<SelectListItem>
                    {
                        new SelectListItem() { Text = S["All cultures"], Value = "" }
                    };
                    cultures.AddRange(supportedCultures.Select(culture => new SelectListItem() { Text = culture, Value = culture }));

                    m.Cultures = cultures;
                }).Location("Actions:20");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            var viewModel = new LocalizationContentsAdminFilterViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, "Localization"))
            {
                if (viewModel.ShowLocalizedContentTypes)
                {
                    model.RouteValues.TryAdd("Localization.ShowLocalizedContentTypes", viewModel.ShowLocalizedContentTypes);
                }

                if (!string.IsNullOrEmpty(viewModel.SelectedCulture))
                {
                    model.RouteValues.TryAdd("Localization.SelectedCulture", viewModel.SelectedCulture);
                }
            }

            return Edit(model, updater);
        }
    }
}
