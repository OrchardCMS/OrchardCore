using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class MatchesProvider : IValidationRuleProvider
    {
        public int Index => 16;
        public string DisplayName => "Matches";
        public string Name => "Matches";
        public bool IsShowOption => true;
        public string OptionPlaceHolder => "^\\\\d{n}$";
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            var result = false;
            if (!string.IsNullOrEmpty(model.Option))
            {
                model.Option = model.Option.Replace("|-BackslashPlaceholder-|", "\\");
                result = Regex.IsMatch(model.Input, model.Option);
            }
            return Task.FromResult(result);
        }
    }
}
