using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers;

public class FormElementLabelPartDisplayDriver : ContentPartDisplayDriver<FormElementLabelPart>
{
    public override IDisplayResult Display(FormElementLabelPart part)
    {
        return View("FormElementLabelPart", part).Location("Detail", "Content:before");
    }

    public override IDisplayResult Edit(FormElementLabelPart part)
    {
        return Initialize<FormElementLabelPartViewModel>("FormElementLabelPart_Fields_Edit", m =>
        {
            m.Label = part.Label;
            m.LabelOption = part.Option;
        });
    }

    public async override Task<IDisplayResult> UpdateAsync(FormElementLabelPart part, IUpdateModel updater)
    {
        var viewModel = new FormElementLabelPartViewModel();

        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            part.Label = viewModel.Label;
            part.Option = viewModel.LabelOption;
        }

        return Edit(part);
    }
}
