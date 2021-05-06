using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.ViewModels;

namespace OrchardCore.Forms.Drivers
{
    public class ValidationRulePartDisplayDriver : ContentPartDisplayDriver<ValidationRulePart>
    {
        private readonly IEnumerable<IValidationRuleProvider> _validationRuleProviders;
        private readonly List<ValidationOptionViewModel> validationOptionViewModels =new List<ValidationOptionViewModel>();

        public ValidationRulePartDisplayDriver(IEnumerable<IValidationRuleProvider> validationRuleProviders)
        {
            _validationRuleProviders = validationRuleProviders.OrderBy(a=>a.Index);
            foreach (var provider in _validationRuleProviders)
            {
                validationOptionViewModels.Add(new ValidationOptionViewModel()
                {
                    DisplayName = provider.DisplayName,
                    Name = provider.Name,
                    IsShowOption = provider.IsShowOption,
                    OptionPlaceHolder = provider.OptionPlaceHolder,
                    IsShowErrorMessage = provider.IsShowErrorMessage,
                });
            }
        }
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
                m.ErrorMessage = part.ErrorMessage;
                m.ValidationOptionViewModels = validationOptionViewModels;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(ValidationRulePart part, IUpdateModel updater)
        {
            var viewModel = new ValidationRulePartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Type = viewModel.Type?.Trim();
                part.Option = viewModel.Option?.Trim();
                part.ErrorMessage = viewModel.ErrorMessage?.Trim();
            }

            return Edit(part);
        }
    }
}
