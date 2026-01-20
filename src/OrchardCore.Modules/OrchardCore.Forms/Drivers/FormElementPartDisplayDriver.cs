using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class FormElementPartDisplayDriver : ContentPartDisplayDriver<FormElementPart>
{
    public override IDisplayResult Edit(FormElementPart part, BuildPartEditorContext context)
    {
        return Initialize<FormElementPartEditViewModel>("FormElementPart_Fields_Edit", m =>
        {
            m.Id = part.Id;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(FormElementPart part, UpdatePartEditorContext context)
    {
        var viewModel = new FormElementPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Id = viewModel.Id?.Trim();

        return Edit(part, context);
    }
}
