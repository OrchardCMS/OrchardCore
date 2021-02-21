using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Liquid
{
    public class LiquidFilterResolver<TLiquidFilter> where TLiquidFilter : class, ILiquidFilter
    {
        public ValueTask<FluidValue> ResolveThenProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;
            var filter = services.GetRequiredService<TLiquidFilter>();

            return filter.ProcessAsync(input, arguments, context);
        }
    }
}
