using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class ButtonPartDisplayDriver : ContentPartDisplayDriver<ButtonPart>
    {
        public override IDisplayResult Display(ButtonPart part)
        {
            return View("ButtonPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(ButtonPart part)
        {
            return Initialize<ButtonPartEditViewModel>("ButtonPart_Fields_Edit", m =>
            {
                m.Text = part.Text;
                m.Type = part.Type;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(ButtonPart part, IUpdateModel updater)
        {
            var viewModel = new ButtonPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Text = viewModel.Text?.Trim();
                part.Type = viewModel.Type?.Trim();
            }

            return Edit(part);
        }
    }
}
