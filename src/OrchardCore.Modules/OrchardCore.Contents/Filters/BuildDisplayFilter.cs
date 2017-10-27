using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Filters
{
    public class BuildDisplayFilter : ILiquidFilter
    {
        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                return NilValue.Instance;
            }

            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'build_display'");
            }

            var displayManager = ((IServiceProvider)services).GetRequiredService<IContentItemDisplayManager>();

            return FluidValue.Create(await displayManager.BuildDisplayAsync(contentItem, null, arguments.At(0).ToStringValue()));
        }
    }
}
