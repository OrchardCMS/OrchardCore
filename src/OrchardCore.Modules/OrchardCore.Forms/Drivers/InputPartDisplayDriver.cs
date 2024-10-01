using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class InputPartDisplayDriver : ContentPartDisplayDriver<InputPart>
{
    public override IDisplayResult Display(InputPart part, BuildPartDisplayContext context)
    {
        return View("InputPart", part).Location("Detail", "Content");
    }

    public override IDisplayResult Edit(InputPart part, BuildPartEditorContext context)
    {
        return Initialize<InputPartEditViewModel>("InputPart_Fields_Edit", m =>
        {
            m.Placeholder = part.Placeholder;
            m.DefaultValue = part.DefaultValue;
            m.Type = part.Type;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(InputPart part, UpdatePartEditorContext context)
    {
        var viewModel = new InputPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Placeholder = viewModel.Placeholder?.Trim();
        part.DefaultValue = viewModel.DefaultValue?.Trim();
        part.Type = viewModel.Type?.Trim();

        return Edit(part, context);
    }
}
