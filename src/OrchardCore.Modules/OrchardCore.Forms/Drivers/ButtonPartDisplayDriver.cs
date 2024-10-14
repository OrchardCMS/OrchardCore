using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public sealed class ButtonPartDisplayDriver : ContentPartDisplayDriver<ButtonPart>
{
    public override IDisplayResult Display(ButtonPart part, BuildPartDisplayContext context)
    {
        return View("ButtonPart", part)
            .Location("Detail", "Content");
    }

    public override IDisplayResult Edit(ButtonPart part, BuildPartEditorContext context)
    {
        return Initialize<ButtonPartEditViewModel>("ButtonPart_Fields_Edit", m =>
        {
            m.Text = part.Text;
            m.Type = part.Type;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ButtonPart part, UpdatePartEditorContext context)
    {
        var viewModel = new ButtonPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Text = viewModel.Text?.Trim();
        part.Type = viewModel.Type?.Trim();

        return Edit(part, context);
    }
}
