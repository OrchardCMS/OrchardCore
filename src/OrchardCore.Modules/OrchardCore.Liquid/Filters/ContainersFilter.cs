using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;

namespace OrchardCore.Liquid.Filters
{
    public class ContainersFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public ContainersFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                throw new ArgumentException("A Content Item was expected");
            }

            var containers = new List<ObjectValue>();

            string containerId = contentItem.Content?.ContainedPart?.ListContentItemId;

            while (containerId != null)
            {
                contentItem = await _contentManager.GetAsync(containerId);

                if (contentItem != null)
                {
                    containers.Add(new ObjectValue(contentItem));
                    containerId = contentItem.Content?.ContainedPart?.ListContentItemId;
                }
                else
                {
                    containerId = null;
                }    
            }

            return new ArrayValue(containers);
        }
    }
}
