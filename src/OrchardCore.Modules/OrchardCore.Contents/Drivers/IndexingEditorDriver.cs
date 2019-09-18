using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class IndexingEditorDriver : ContentPartDisplayDriver<IndexingPart>
    {
        public override IDisplayResult Edit(IndexingPart part, BuildPartEditorContext context)
        {
            return Initialize<IndexingEditorViewModel>("IndexingPart_Edit", model =>
            {
                model.IsIndexed = part.IsIndexed;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(IndexingPart part, UpdatePartEditorContext context)
        {
            var model = new IndexingEditorViewModel();
            await context.Updater.TryUpdateModelAsync(model, Prefix);
            part.IsIndexed = model.IsIndexed;

            return Edit(part, context);
        }
    }
}
