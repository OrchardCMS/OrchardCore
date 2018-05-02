using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.Drivers
{
    public class FormPartDisplay : ContentPartDisplayDriver<FormPart>
    {
        public override IDisplayResult Edit(FormPart part, BuildPartEditorContext context)
        {
            return View("FormPart_Edit", part);
        }

        public async override Task<IDisplayResult> UpdateAsync(FormPart part, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(part, Prefix);
            return Edit(part);
        }
    }
}
