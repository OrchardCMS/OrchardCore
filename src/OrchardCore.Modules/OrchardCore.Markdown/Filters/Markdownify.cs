using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Services;

namespace OrchardCore.Markdown.Filters
{
    public static class Markdownify
    {
        public static ValueTask<FluidValue> Markdown(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var markdownService = context.Services.GetRequiredService<IMarkdownService>();

            return new ValueTask<FluidValue>(new StringValue(markdownService.ToHtml(input.ToStringValue())));
        }
    }
}
