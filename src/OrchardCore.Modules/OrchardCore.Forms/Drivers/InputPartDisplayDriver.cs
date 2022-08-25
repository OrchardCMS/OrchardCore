using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Forms.Drivers
{
    public class InputPartDisplayDriver : ContentPartDisplayDriver<InputPart>
    {
        private readonly IResourceManager _resourceManager;

        public InputPartDisplayDriver(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public override IDisplayResult Display(InputPart part)
        {
            return View("InputPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(InputPart part)
        {
            ResourceManagementOptionsConfiguration.InjectEditFormWidgetOptions(_resourceManager);

            return Initialize<InputPartEditViewModel>("InputPart_Fields_Edit", m =>
            {
                m.Placeholder = part.Placeholder;
                m.DefaultValue = part.DefaultValue;
                m.Type = part.Type;
                m.LabelOption = part.LabelOption;
                m.Label = part.Label;
                m.ValidationOption = part.ValidationOption;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(InputPart part, IUpdateModel updater)
        {
            var viewModel = new InputPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Placeholder = viewModel.Placeholder?.Trim();
                part.DefaultValue = viewModel.DefaultValue?.Trim();
                part.Type = viewModel.Type?.Trim();
                part.LabelOption = viewModel.LabelOption;
                part.Label = viewModel.Label;
                part.ValidationOption = viewModel.ValidationOption;
            }

            return Edit(part);
        }
    }
}
