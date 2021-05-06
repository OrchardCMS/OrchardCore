using System.Threading.Tasks;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Services
{
    public interface IValidationRuleService
    {
        Task<bool> ValidateInputByRuleAsync(ValidationRuleInput input);
    }
}
