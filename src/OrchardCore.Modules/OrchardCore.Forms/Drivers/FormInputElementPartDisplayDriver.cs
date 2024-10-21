using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Forms.Drivers;

public sealed class FormInputElementPartDisplayDriver : ContentPartDisplayDriver<FormInputElementPart>
{
    internal readonly IStringLocalizer S;

    public FormInputElementPartDisplayDriver(IStringLocalizer<FormInputElementPartDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(FormInputElementPart part, BuildPartEditorContext context)
    {
        return Initialize<FormInputElementPartEditViewModel>("FormInputElementPart_Fields_Edit", m =>
        {
            m.Name = part.Name;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(FormInputElementPart part, UpdatePartEditorContext context)
    {
        var viewModel = new FormInputElementPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        if (string.IsNullOrWhiteSpace(viewModel.Name))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Name), S["A value is required for Name."]);
        }

        part.Name = viewModel.Name?.Trim();
        part.ContentItem.DisplayText = part.Name;

        return Edit(part, context);
    }
}
