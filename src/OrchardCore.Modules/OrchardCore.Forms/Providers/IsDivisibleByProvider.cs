using System;
using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsDivisibleByProvider : IValidationRuleProvider
    {
        public int Index => 9;
        public string DisplayName => "Is Divisible By";
        public string Name => "IsDivisibleBy";
        public bool IsShowOption => true;
        public string OptionPlaceHolder => "3";
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            var result = false;
            if (Single.TryParse(model.Input, out var originalNumber) && Int32.TryParse(model.Option, out var divisor))
            {
                if (divisor != 0) result = originalNumber % divisor == 0;
            }
            return Task.FromResult(result);
        }
    }
}
