using System;
using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsDecimalProvider : IValidationRuleProvider
    {
        public int Index => 8;
        public string DisplayName => "Is Decimal";
        public string Name => "IsDecimal";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(ValidationRuleHelpers.ValidateIs<decimal>(model.Input));
        }
    }
}
