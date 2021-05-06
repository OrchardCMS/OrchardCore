using System.Text;
using System.Threading.Tasks;
using OrchardCore.Forms.Helpers;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsByteLengthProvider : IValidationRuleProvider
    {
        public int Index => 6;
        public string DisplayName => "Is ByteLength";
        public string Name => "IsByteLength";
        public bool IsShowOption => true;
        public string OptionPlaceHolder => "{&quot;min&quot; : 0, &quot;max&quot; : 20}";
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            return Task.FromResult(ValidationRuleHelpers.ValidateLength(Encoding.UTF8.GetByteCount(model.Input), model.Option));
        }
    }
}
