using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class TextAreaPartDisplayDriver : ContentPartDisplayDriver<TextAreaPart>
{
    private const int DefaultRows = 3;

    public override IDisplayResult Display(TextAreaPart part, BuildPartDisplayContext context)
    {
        return View("TextAreaPart", part).Location(OrchardCoreConstants.DisplayType.Detail, "Content");
    }

    public override IDisplayResult Edit(TextAreaPart part, BuildPartEditorContext context)
    {
        return Initialize<TextAreaPartEditViewModel>("TextAreaPart_Fields_Edit", m =>
        {
            m.Placeholder = part.Placeholder;
            m.DefaultValue = part.DefaultValue;
            m.Rows = part.Rows;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(TextAreaPart part, UpdatePartEditorContext context)
    {
        var viewModel = new TextAreaPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Placeholder = viewModel.Placeholder?.Trim();
        part.DefaultValue = viewModel.DefaultValue?.Trim();
        part.Rows = viewModel.Rows > DefaultRows
            ? viewModel.Rows
            : DefaultRows;

        return Edit(part, context);
    }
}
