using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public class ContentItemFilter : ILiquidFilter
    {
        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'shape_build_display'");
            }

            var contentManager = ((IServiceProvider)services).GetRequiredService<IContentManager>();

            if (input.Type == FluidValues.Array)
            {
                // List of content item ids

                var contentItemIds = input.Enumerate().Select(x => x.ToStringValue()).ToArray();

                return FluidValue.Create(await contentManager.GetAsync(contentItemIds));
            }
            else
            {
                var contentItemId = input.ToString();

                return FluidValue.Create(await contentManager.GetAsync(contentItemId));
            }
        }
    }
}
