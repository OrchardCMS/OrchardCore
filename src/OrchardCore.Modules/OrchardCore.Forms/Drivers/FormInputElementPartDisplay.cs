using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class FormInputElementPartDisplay : ContentPartDisplayDriver<FormInputElementPart>
    {
        public override IDisplayResult Edit(FormInputElementPart part, BuildPartEditorContext context)
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
                part.Name = viewModel.Name?.Trim();
                part.ContentItem.DisplayText = part.Name;
            }

            return Edit(part);
        }
    }
}
