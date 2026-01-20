using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Liquid
{
    public class LiquidFilterDelegateResolver<TLiquidFilter> where TLiquidFilter : class, ILiquidFilter
    {
        public ValueTask<FluidValue> ResolveAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var ctx = context as LiquidTemplateContext
                ?? throw new InvalidOperationException("An implementation of 'LiquidTemplateContext' is required");

            var services = ctx.Services;
            var filter = services.GetRequiredService<TLiquidFilter>();

            return filter.ProcessAsync(input, arguments, ctx);
        }
    }
}
