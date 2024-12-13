using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class FormElementLabelPartDisplayDriver : ContentPartDisplayDriver<FormElementLabelPart>
{
    public override IDisplayResult Display(FormElementLabelPart part, BuildPartDisplayContext context)
    {
        return View("FormElementLabelPart", part)
            .Location("Detail", "Content:before");
    }

    public override IDisplayResult Edit(FormElementLabelPart part, BuildPartEditorContext context)
    {
        return Initialize<FormElementLabelPartViewModel>("FormElementLabelPart_Fields_Edit", m =>
        {
            m.Label = part.Label;
            m.LabelOption = part.Option;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(FormElementLabelPart part, UpdatePartEditorContext context)
    {
        var viewModel = new FormElementLabelPartViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Label = viewModel.Label;
        part.Option = viewModel.LabelOption;

        return Edit(part, context);
    }
}
