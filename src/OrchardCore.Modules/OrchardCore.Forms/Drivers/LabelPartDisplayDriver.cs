using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class LabelPartDisplayDriver : ContentPartDisplayDriver<LabelPart>
    {
        public override IDisplayResult Display(LabelPart part)
        {
            return View("LabelPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(LabelPart part, BuildPartEditorContext context)
        {
            return Initialize<LabelPartEditViewModel>("LabelPart_Fields_Edit", m =>
            {
                m.For = part.For;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(LabelPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var viewModel = new LabelPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.For = viewModel.For?.Trim();
            }

            return Edit(part, context);
        }
    }
}
