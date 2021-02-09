using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public static class SanitizeHtmlFilter
    {
        public static ValueTask<FluidValue> SanitizeHtml(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var html = input.ToStringValue();

            var sanitizer = context.Services.GetRequiredService<IHtmlSanitizerService>();
            html = sanitizer.Sanitize(html);

            return new ValueTask<FluidValue>(new StringValue(html));
        }
    }
}
