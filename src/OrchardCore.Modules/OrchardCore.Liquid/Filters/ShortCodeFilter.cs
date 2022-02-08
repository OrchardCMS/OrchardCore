using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Liquid.Filters
{
    public class ShortcodeFilter : ILiquidFilter
    {
        private readonly IShortcodeService _shortcodeService;

        public ShortcodeFilter(IShortcodeService shortcodeService)
        {
            _shortcodeService = shortcodeService;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
        {
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

            return new StringValue(await _shortcodeService.ProcessAsync(input.ToStringValue(), shortcodeContext));
        }
    }
}
