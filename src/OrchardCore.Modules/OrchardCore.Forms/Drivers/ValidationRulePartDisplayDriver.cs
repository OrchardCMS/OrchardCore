using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class ValidationRulePartDisplayDriver : ContentPartDisplayDriver<ValidationRulePart>
    {
        public override IDisplayResult Display(ValidationRulePart part)
        {
            return View("ValidationRulePart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(ValidationRulePart part)
        {
            return Initialize<ValidationRulePartEditViewModel>("ValidationRulePart_Fields_Edit", m =>
            {
                m.Type = part.Type;
                m.Option = part.Option;
                m.ValidationMessage = part.ValidationMessage;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(ValidationRulePart part, IUpdateModel updater)
        {
            var viewModel = new ValidationRulePartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Type = viewModel.Type?.Trim();
                part.Option = viewModel.Option?.Trim();
                part.ValidationMessage = viewModel.ValidationMessage?.Trim();
            }

            return Edit(part);
        }
    }
}
