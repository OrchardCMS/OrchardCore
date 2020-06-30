using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Liquid.Filters
{
    public class ShortcodeFilter : ILiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'shortcode'");
            }

            var shortCodeService = ((IServiceProvider)services).GetRequiredService<IShortcodeService>();

            return new StringValue(await shortCodeService.ProcessAsync(input.ToStringValue()));
        }
    }
}
