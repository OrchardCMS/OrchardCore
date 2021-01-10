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
    public class ShortcodeFilter : ILiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'shortcode'");
            }

            var shortcodeService = ((IServiceProvider)services).GetRequiredService<IShortcodeService>();

            var context = new Context();

            // Retrieve the 'ContentItem' from the ambient liquid scope.
            var model = ctx.LocalScope.GetValue("Model").ToObjectValue();
            if (model is Shape shape && shape.Properties.TryGetValue("ContentItem", out var contentItem))
            {
                context["ContentItem"] = contentItem;
            }
            else
            {
                context["ContentItem"] = null;
            }

            return new StringValue(await shortcodeService.ProcessAsync(input.ToStringValue(), context));
        }
    }
}
