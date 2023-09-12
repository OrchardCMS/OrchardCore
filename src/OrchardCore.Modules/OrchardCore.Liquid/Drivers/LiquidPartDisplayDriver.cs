using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid.Models;
using OrchardCore.Liquid.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Liquid.Drivers
{
    public class LiquidPartDisplayDriver : ContentPartDisplayDriver<LiquidPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplatemanager;
        protected readonly IStringLocalizer S;

        public LiquidPartDisplayDriver(ILiquidTemplateManager liquidTemplatemanager, IStringLocalizer<LiquidPartDisplayDriver> localizer)
        {
            _liquidTemplatemanager = liquidTemplatemanager;
            S = localizer;
        }

        public override IDisplayResult Display(LiquidPart liquidPart)
        {
            return Combine(
                Initialize<LiquidPartViewModel>("LiquidPart", m => BuildViewModel(m, liquidPart))
                    .Location("Detail", "Content"),
                Initialize<LiquidPartViewModel>("LiquidPart_Summary", m => BuildViewModel(m, liquidPart))
                    .Location("Summary", "Content")
            );
        }

        public override IDisplayResult Edit(LiquidPart liquidPart)
        {
            return Initialize<LiquidPartViewModel>("LiquidPart_Edit", m => BuildViewModel(m, liquidPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(LiquidPart model, IUpdateModel updater)
        {
            var viewModel = new LiquidPartViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Liquid))
            {
                if (!String.IsNullOrEmpty(viewModel.Liquid) && !_liquidTemplatemanager.Validate(viewModel.Liquid, out var errors))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.Liquid), S["The Liquid Body doesn't contain a valid Liquid expression. Details: {0}", String.Join(" ", errors)]);
                }
                else
                {
                    model.Liquid = viewModel.Liquid;
                }
            }

            return Edit(model);
        }

        private static void BuildViewModel(LiquidPartViewModel model, LiquidPart liquidPart)
        {
            model.Liquid = liquidPart.Liquid;
            model.LiquidPart = liquidPart;
            model.ContentItem = liquidPart.ContentItem;
        }
    }
}
