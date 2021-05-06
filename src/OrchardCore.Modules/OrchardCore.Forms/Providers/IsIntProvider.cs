using System;
using System.Threading.Tasks;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsIntProvider : IValidationRuleProvider
    {
        public int Index => 12;
        public string DisplayName => "Is Int";
        public string Name => "IsInt";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(ValidationRuleHelpers.ValidateIs<int>(model.Input));
        }
    }
}
