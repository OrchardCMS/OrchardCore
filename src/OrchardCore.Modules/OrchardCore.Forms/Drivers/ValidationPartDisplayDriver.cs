using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class ValidationPartDisplayDriver : ContentPartDisplayDriver<ValidationPart>
{
    public override IDisplayResult Display(ValidationPart part, BuildPartDisplayContext context)
    {
        return View("ValidationPart", part).Location("Detail", "Content");
    }

    public override IDisplayResult Edit(ValidationPart part, BuildPartEditorContext context)
    {
        return Initialize<ValidationPartEditViewModel>("ValidationPart_Fields_Edit", m =>
        {
            m.For = part.For;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ValidationPart part, UpdatePartEditorContext context)
    {
        var viewModel = new ValidationPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.For = viewModel.For?.Trim();

        return Edit(part, context);
    }
}
