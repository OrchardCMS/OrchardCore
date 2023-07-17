using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Forms.Drivers
{
    public class FormInputElementPartDisplayDriver : ContentPartDisplayDriver<FormInputElementPart>
    {
        protected readonly IStringLocalizer S;

        public FormInputElementPartDisplayDriver(IStringLocalizer<FormInputElementPartDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(FormInputElementPart part)
        {
            return Initialize<FormInputElementPartEditViewModel>("FormInputElementPart_Fields_Edit", m =>
            {
                m.Name = part.Name;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(FormInputElementPart part, IUpdateModel updater)
        {
            var viewModel = new FormInputElementPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                if (String.IsNullOrWhiteSpace(viewModel.Name))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(viewModel.Name), S["A value is required for Name."]);
                }

                part.Name = viewModel.Name?.Trim();
                part.ContentItem.DisplayText = part.Name;
            }

            return Edit(part);
        }
    }
}
