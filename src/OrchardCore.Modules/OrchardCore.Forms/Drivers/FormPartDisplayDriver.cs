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
    public class FormPartDisplayDriver : ContentPartDisplayDriver<FormPart>
    {
        public const string DefaultFormLocationInputName = "__RequestOriginatedFrom";

        protected readonly IStringLocalizer S;

        public FormPartDisplayDriver(IStringLocalizer<FormPartDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(FormPart part)
        {
            return Initialize<FormPartEditViewModel>("FormPart_Fields_Edit", m =>
            {
                m.Action = part.Action;
                m.Method = part.Method;
                m.WorkflowTypeId = part.WorkflowTypeId;
                m.EncType = part.EncType;
                m.EnableAntiForgeryToken = part.EnableAntiForgeryToken;
                m.SaveFormLocation = part.SaveFormLocation;
                m.FormLocationKey = part.FormLocationKey ?? DefaultFormLocationInputName;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(FormPart part, IUpdateModel updater)
        {
            var viewModel = new FormPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Action = viewModel.Action?.Trim();
                part.Method = viewModel.Method;
                part.WorkflowTypeId = viewModel.WorkflowTypeId;
                part.EncType = viewModel.EncType;
                part.EnableAntiForgeryToken = viewModel.EnableAntiForgeryToken;
                part.SaveFormLocation = false;
                part.FormLocationKey = string.Empty;

                if (viewModel.SaveFormLocation)
                {
                    part.SaveFormLocation = true;

                    if (string.IsNullOrWhiteSpace(viewModel.FormLocationKey))
                    {
                        updater.ModelState.AddModelError(Prefix, nameof(viewModel.FormLocationKey), S["Form Location Key is required."]);
                    }
                    else
                    {
                        part.FormLocationKey = viewModel.FormLocationKey.Trim();
                    }
                }
            }

            return Edit(part);
        }
    }
}
