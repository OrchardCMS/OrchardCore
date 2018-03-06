using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Queries.Liquid
{
    public class QueryParametersFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var queryValue = input as QueryValue;

            if (queryValue == null)
            {
                return Task.FromResult<FluidValue>(NilValue.Instance);
            }

            foreach (var name in arguments.Names)
            {
                queryValue.Parameters.Add(name, arguments[name].ToObjectValue());
            }

            return Task.FromResult<FluidValue>(queryValue);
        }
    }
}