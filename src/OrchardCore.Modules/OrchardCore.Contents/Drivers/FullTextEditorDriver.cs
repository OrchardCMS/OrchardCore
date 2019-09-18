using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class FullTextEditorDriver : ContentPartDisplayDriver<FullTextPart>
    {
        public override IDisplayResult Edit(FullTextPart part, BuildPartEditorContext context)
        {
            return Initialize<FullTextEditorViewModel>("FullTextPart_Edit", model =>
            {
                model.IsIndexed = part.IsIndexed;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(FullTextPart part, UpdatePartEditorContext context)
        {
            var model = new FullTextEditorViewModel();
            await context.Updater.TryUpdateModelAsync(model, Prefix);
            part.IsIndexed = model.IsIndexed;

            return Edit(part, context);
        }
    }
}