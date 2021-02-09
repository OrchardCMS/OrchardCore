using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public static class ContentItemFilter
    {
        public static async ValueTask<FluidValue> ContentItemId(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var contentManager = context.Services.GetRequiredService<IContentManager>();

            if (input.Type == FluidValues.Array)
            {
                // List of content item ids
                var contentItemIds = input.Enumerate().Select(x => x.ToStringValue()).ToArray();

                return FluidValue.Create(await contentManager.GetAsync(contentItemIds), ctx.Options);
            }
            else
            {
                var contentItemId = input.ToStringValue();

                return FluidValue.Create(await contentManager.GetAsync(contentItemId), ctx.Options);
            }
        }
    }
}
