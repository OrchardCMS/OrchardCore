using System;
using System.Threading.Tasks;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsEmptyProvider : IValidationRuleProvider
    {
        public int Index => 10;
        public string DisplayName => "Is Empty";
        public string Name => "IsEmpty";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(String.IsNullOrEmpty(model.Input));
        }
    }
}
