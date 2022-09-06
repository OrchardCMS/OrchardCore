using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public class FormElementValidationPartDisplayDriver : ContentPartDisplayDriver<FormElementValidationPart>
{
    public override IDisplayResult Display(FormElementValidationPart part)
    {
        return View("FormElementValidationPart", part).Location("Detail", "Content:after");
    }

    public override IDisplayResult Edit(FormElementValidationPart part)
    {
        return Initialize<FormElementValidationPartViewModel>("FormElementValidationPart_Fields_Edit", m =>
        {
            m.ValidationOption = part.Option;
        });
    }

    public async override Task<IDisplayResult> UpdateAsync(FormElementValidationPart part, IUpdateModel updater)
    {
        var viewModel = new FormElementValidationPartViewModel();

        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            part.Option = viewModel.ValidationOption;
        }

        return Edit(part);
    }
}
