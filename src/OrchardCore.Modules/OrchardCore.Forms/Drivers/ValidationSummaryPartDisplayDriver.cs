using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class ValidationSummaryPartDisplayDriver : ContentPartDisplayDriver<ValidationSummaryPart>
    {
        public override IDisplayResult Display(ValidationSummaryPart part)
        {
            return View("ValidationSummaryPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(ValidationSummaryPart part)
        {
            return Initialize<ValidationSummaryViewModel>("ValidationSummaryPart_Fields_Edit", model =>
            {
                model.ModelOnly = part.ModelOnly;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(ValidationSummaryPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var model = new ValidationSummaryViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                part.ModelOnly = model.ModelOnly;
            }

            return Edit(part);
        }
    }
}
