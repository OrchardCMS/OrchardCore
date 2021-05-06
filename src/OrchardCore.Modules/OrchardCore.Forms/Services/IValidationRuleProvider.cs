using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Services
{
    public interface IValidationRuleProvider
    {
        public int Index => 0;
        public string DisplayName => String.Empty;
        public string Name => String.Empty;
        public bool IsShowOption => true;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => true;
        Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model);
    }
}
