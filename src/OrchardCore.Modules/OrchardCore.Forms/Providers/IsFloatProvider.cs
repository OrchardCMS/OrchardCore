using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsFloatProvider : IValidationRuleProvider
    {
        public int Index => 11;
        public string DisplayName => "Is Float";
        public string Name => "IsFloat";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => "{&quot;min&quot; : 7.22, &quot;max&quot; : 9.55}";
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            var result = false;
            if (Single.TryParse(model.Input, out var original))
            {
                float min;
                var obj = JToken.Parse(model.Option);
                Single.TryParse(obj["max"]?.ToString(), out var max);
                Single.TryParse(obj["min"]?.ToString(), out min);
                if (original >= min && (max == 0 || original <= max)) result = true;
            }
            return Task.FromResult(result);
        }
    }
}
