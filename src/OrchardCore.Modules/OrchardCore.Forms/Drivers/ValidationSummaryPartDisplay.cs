using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.Drivers
{
    public class ValidationSummaryPartDisplay : ContentPartDisplayDriver<ValidationSummaryPart>
    {
        public override IDisplayResult Display(ValidationSummaryPart part)
        {
            return View("ValidationSummaryPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(ValidationSummaryPart part)
        {
            return View("ValidationSummaryPart_Fields_Edit", part);
        }
    }
}
