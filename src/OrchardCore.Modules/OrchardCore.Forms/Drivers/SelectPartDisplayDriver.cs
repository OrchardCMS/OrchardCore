using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class SelectPartDisplayDriver : ContentPartDisplayDriver<SelectPart>
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        private readonly IStringLocalizer S;

        public SelectPartDisplayDriver(IStringLocalizer<SelectPartDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Display(SelectPart part)
        {
            return View("SelectPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(SelectPart part)
        {
            return Initialize<SelectPartEditViewModel>("SelectPart_Fields_Edit", m =>
            {
                m.Options = JsonConvert.SerializeObject(part.Options ?? Array.Empty<SelectOption>(), SerializerSettings);
                m.DefaultValue = part.DefaultValue;
                m.Editor = part.Editor;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(SelectPart part, IUpdateModel updater)
        {
            var viewModel = new SelectPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.DefaultValue = viewModel.DefaultValue;
                try
                {
                    part.Editor = viewModel.Editor;
                    part.Options = String.IsNullOrWhiteSpace(viewModel.Options)
                        ? Array.Empty<SelectOption>()
                        : JsonConvert.DeserializeObject<SelectOption[]>(viewModel.Options);
                }
                catch
                {
                    updater.ModelState.AddModelError(Prefix + '.' + nameof(SelectPartEditViewModel.Options), S["The options are written in an incorrect format."]);
                }
            }

            return Edit(part);
        }
    }
}
