using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsNumericProvider : IValidationRuleProvider
    {
        public int Index => 15;
        public string DisplayName => "Is Numeric";
        public string Name => "IsNumeric";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            var exp = @"^[0-9]+$";
            return Task.FromResult(Regex.IsMatch(model.Input, exp));
        }
    }
}
