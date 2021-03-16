using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public class SanitizeHtmlFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var html = input.ToStringValue();

            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'sanitize'");
            }

            var sanitizer = ((IServiceProvider)services).GetRequiredService<IHtmlSanitizerService>();
            html = sanitizer.Sanitize(html);

            return new ValueTask<FluidValue>(new StringValue(html));
        }
    }
}
