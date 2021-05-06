using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Forms.Services;
using OrchardCore.Forms.Services.Models;

namespace OrchardCore.Forms.Providers
{
    public class IsJSONProvider : IValidationRuleProvider
    {
        public int Index => 13;
        public string DisplayName => "Is JSON";
        public string Name => "IsJSON";
        public bool IsShowOption => false;
        public string OptionPlaceHolder => String.Empty;
        public bool IsShowErrorMessage => true;
        public Task<bool> ValidateInputByRuleAsync(ValidationRuleInput model)
        {
            var result = false;
            if (!String.IsNullOrWhiteSpace(model.Input))
            {
                var value = model.Input.Trim();
                if ((value.StartsWith("{") && value.EndsWith("}")) || // For object.
                    (value.StartsWith("[") && value.EndsWith("]"))) // For array.
                {
                    try
                    {
                        var ob = JToken.Parse(value);
                        result = true;
                    }
                    catch (JsonReaderException)
                    {
                        
                    }
                }
            }
            return Task.FromResult(result);
        }
    }
}
