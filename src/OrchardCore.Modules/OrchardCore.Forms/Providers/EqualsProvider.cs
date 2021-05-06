using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class EqualsProvider : IValidationRuleProvider
    {
        public int Index => 2;
        public string DisplayName => "Equals";
        public string Name => "Equals";
        public bool IsShowOption => true;
        public string OptionPlaceHolder => "string to compare with input";
        public bool IsShowErrorMessage => true;

        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(model.Input.Equals(model.Option, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
