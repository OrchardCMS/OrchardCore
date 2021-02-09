using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Liquid.Filters
{
    public static class ShortcodeFilter
    {
        public static async ValueTask<FluidValue> Shortcode(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var shortcodeService = context.Services.GetRequiredService<IShortcodeService>();

            var shortcodeContext = new Context();

            // Retrieve the 'ContentItem' from the ambient liquid scope.
            var model = context.GetValue("Model").ToObjectValue();
            if (model is Shape shape && shape.Properties.TryGetValue("ContentItem", out var contentItem))
            {
                shortcodeContext["ContentItem"] = contentItem;
            }
            else
            {
                shortcodeContext["ContentItem"] = null;
            }

            return new StringValue(await shortcodeService.ProcessAsync(input.ToStringValue(), shortcodeContext));
        }
    }
}
