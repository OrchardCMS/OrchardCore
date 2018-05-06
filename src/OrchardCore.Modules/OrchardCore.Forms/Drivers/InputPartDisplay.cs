using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class InputPartDisplay : ContentPartDisplayDriver<InputPart>
    {
        public override IDisplayResult Display(InputPart part)
        {
            return View("InputPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(InputPart part, BuildPartEditorContext context)
        {
            return Initialize<InputPartEditViewModel>("InputPart_Edit", m =>
            {
                m.Name = part.Name;
                m.Placeholder = part.Placeholder;
                m.Label = part.Label;
                m.DefaultValue = part.DefaultValue;
                m.Type = part.Type;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(InputPart part, IUpdateModel updater)
        {
            var viewModel = new InputPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Name = viewModel.Name?.Trim();
                part.Placeholder = viewModel.Placeholder?.Trim();
                part.Label = viewModel.Label?.Trim();
                part.DefaultValue = viewModel.DefaultValue?.Trim();
                part.Type = viewModel.Type?.Trim();
            }

            return Edit(part);
        }
    }
}
