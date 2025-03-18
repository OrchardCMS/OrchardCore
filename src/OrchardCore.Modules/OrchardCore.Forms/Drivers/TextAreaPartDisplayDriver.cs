using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Forms.Drivers;

public sealed class TextAreaPartDisplayDriver : ContentPartDisplayDriver<TextAreaPart>
{
    private readonly IStringLocalizer S;

    public TextAreaPartDisplayDriver(IStringLocalizer<TextAreaPartDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

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
        part.Rows = viewModel.Rows;

        if (viewModel.Rows < 1)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Rows), S["The Rows field should be greater than or equal 1."]);
        }

        return Edit(part, context);
    }
}
