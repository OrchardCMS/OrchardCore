using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class NoneProvider : IValidationRuleProvider
    {
        public int Index => 0;
        public string DisplayName => "None";
        public string RuleName => "None";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => false;

        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(true);
        }
    }
}
