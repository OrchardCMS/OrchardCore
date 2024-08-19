using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class LabelPartDisplayDriver : ContentPartDisplayDriver<LabelPart>
{
    public override IDisplayResult Display(LabelPart part, BuildPartDisplayContext context)
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

    public override async Task<IDisplayResult> UpdateAsync(LabelPart part, UpdatePartEditorContext context)
    {
        var viewModel = new LabelPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.For = viewModel.For?.Trim();

        return Edit(part, context);
    }
}
