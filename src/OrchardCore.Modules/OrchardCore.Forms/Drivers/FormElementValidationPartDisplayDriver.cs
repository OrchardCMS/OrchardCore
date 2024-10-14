using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class FormElementValidationPartDisplayDriver : ContentPartDisplayDriver<FormElementValidationPart>
{
    public override IDisplayResult Display(FormElementValidationPart part, BuildPartDisplayContext context)
    {
        return View("FormElementValidationPart", part)
            .Location("Detail", "Content:after");
    }

    public override IDisplayResult Edit(FormElementValidationPart part, BuildPartEditorContext context)
    {
        return Initialize<FormElementValidationPartViewModel>("FormElementValidationPart_Fields_Edit", m =>
        {
            m.ValidationOption = part.Option;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(FormElementValidationPart part, UpdatePartEditorContext context)
    {
        var viewModel = new FormElementValidationPartViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Option = viewModel.ValidationOption;

        return Edit(part, context);
    }
}
