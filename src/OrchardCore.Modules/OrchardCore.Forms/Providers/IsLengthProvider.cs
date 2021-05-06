using System;
using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsLengthProvider : IValidationRuleProvider
    {
        public int Index => 14;
        public string DisplayName => "Is Length";
        public string Name => "IsLength";
        public bool IsShowOption => true;
        public string OptionPlaceHolder => "{&quot;min&quot; : 7.22, &quot;max&quot; : 20.0}";
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(ValidationRuleHelpers.ValidateLength(model.Input.Length, model.Option));
        }
    }
}
