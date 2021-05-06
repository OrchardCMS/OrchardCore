using System;
using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsDateProvider : IValidationRuleProvider
    {
        public int Index => 7;
        public string DisplayName => "Is Date";
        public string Name => "IsDate";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(DateTime.TryParse(model.Input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out _));
        }
    }
}
