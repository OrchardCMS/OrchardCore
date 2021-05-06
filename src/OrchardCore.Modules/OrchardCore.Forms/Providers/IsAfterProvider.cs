using System.Threading.Tasks;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsAfterProvider : IValidationRuleProvider
    {
        public int Index => 3;
        public string DisplayName => "Is After";
        public string Name => "IsAfter";
        public bool IsShowOption => true;
        public string OptionPlaceHolder => "2020-03-03";
        public bool IsShowErrorMessage => true;

        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(ValidationRuleHelpers.CompareDatetime(model.Input, model.Option, ">"));
        }
    }
}
