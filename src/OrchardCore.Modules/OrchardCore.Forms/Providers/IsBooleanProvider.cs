using System;
using System.Threading.Tasks;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsBooleanProvider : IValidationRuleProvider
    {
        public int Index => 5;
        public string DisplayName => "Is Boolean";
        public string Name => "IsBoolean";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(Boolean.TryParse(model.Input, out _));
        }
    }
}
