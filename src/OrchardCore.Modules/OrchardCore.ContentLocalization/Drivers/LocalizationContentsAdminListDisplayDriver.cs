using System;
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

        public override IDisplayResult Display(ContentOptionsViewModel model)
            => View("ContentsAdminFilters_Thumbnail__Culture", model).Location("Thumbnail", "Content:20.1");
        
        public override IDisplayResult Edit(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Initialize<LocalizationContentsAdminFilterViewModel>("ContentsAdminList__LocalizationPartFilter", async m =>
                {
                    model.FilterResult.MapTo(m);
                    var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
                    var cultures = new List<SelectListItem>
                    {
                        new SelectListItem() { Text = S["All cultures"], Value = "", Selected = String.IsNullOrEmpty(m.SelectedCulture) }
                    };
                    cultures.AddRange(supportedCultures.Select(culture => new SelectListItem() { Text = culture, Value = culture, Selected = culture == m.SelectedCulture }));

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

                model.FilterResult.MapFrom(viewModel);
            }

            return Edit(model, updater);
        }
    }
}
