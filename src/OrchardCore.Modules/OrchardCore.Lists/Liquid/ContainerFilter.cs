using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Liquid
{
    public class ContainerFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public ContainerFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem ?? throw new ArgumentException("A Content Item was expected");

            var containerId = contentItem.As<ContainedPart>()?.ListContentItemId;

            if (containerId != null)
            {
                var container = await _contentManager.GetAsync(containerId);

                if (container != null)
                {
                    return new ObjectValue(container);
                }
            }

            return new ObjectValue(contentItem);
        }
    }
}
