using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Services
{
    public class ValidationRuleService : IValidationRuleService
    {
        private readonly IEnumerable<IValidationRuleProvider> _validationRuleProviders;

        public ValidationRuleService(IEnumerable<IValidationRuleProvider> validationRuleProviders)
        {
            _validationRuleProviders = validationRuleProviders;
        }
        public async Task<bool> ValidateInputByRuleAsync(ValidationRuleInput input)
        {
            var provider = _validationRuleProviders.FirstOrDefault(a => string.Equals( a.Name, input.Type, StringComparison.OrdinalIgnoreCase));
            return await provider.ValidateInputByRuleAsync(input);
        }
    }
}
