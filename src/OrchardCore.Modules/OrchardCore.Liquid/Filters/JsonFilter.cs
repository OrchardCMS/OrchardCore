using System.Threading.Tasks;
using System.Web;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Liquid.Filters
{
    public class JsonFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var content = input.ToStringValue();

            if (string.IsNullOrWhiteSpace(content))
            {
                return Task.FromResult(input);
            }

            return Task.FromResult<FluidValue>(new StringValue("\"" + HttpUtility.JavaScriptStringEncode(content) + "\""));
        }
    }
}