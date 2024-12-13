using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class TextAreaPartDisplayDriver : ContentPartDisplayDriver<TextAreaPart>
{
    public override IDisplayResult Display(TextAreaPart part, BuildPartDisplayContext context)
    {
        return View("TextAreaPart", part).Location("Detail", "Content");
    }

    public override IDisplayResult Edit(TextAreaPart part, BuildPartEditorContext context)
    {
        return Initialize<TextAreaPartEditViewModel>("TextAreaPart_Fields_Edit", m =>
        {
            m.Placeholder = part.Placeholder;
            m.DefaultValue = part.DefaultValue;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(TextAreaPart part, UpdatePartEditorContext context)
    {
        var viewModel = new InputPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Placeholder = viewModel.Placeholder?.Trim();
        part.DefaultValue = viewModel.DefaultValue?.Trim();

        return Edit(part, context);
    }
}
