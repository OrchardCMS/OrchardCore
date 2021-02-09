using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Liquid
{
    public static class ContainerFilter
    {
        public static async ValueTask<FluidValue> Container(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                throw new ArgumentException("A Content Item was expected");
            }

            var containerId = contentItem.As<ContainedPart>()?.ListContentItemId;

            if (containerId != null)
            {
                var contentManager = context.Services.GetRequiredService<IContentManager>();

                var container = await contentManager.GetAsync(containerId);

                if (container != null)
                {
                    return new ObjectValue(container);
                }
            }

            return new ObjectValue(contentItem);
        }
    }
}
