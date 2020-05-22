using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Services;

namespace OrchardCore.Markdown.Filters
{
    public class Markdownify : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'markdownify'");
            }

            var markdownService = ((IServiceProvider)services).GetRequiredService<IMarkdownService>();

            return new ValueTask<FluidValue>(new StringValue(markdownService.ToHtml(input.ToStringValue())));
        }
    }
}
