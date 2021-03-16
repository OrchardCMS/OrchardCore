using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Lists.Helpers;
using YesSql;

namespace OrchardCore.Lists.Liquid
{
    public class ListCountFilter : ILiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'list_count'");
            }

            string listContentItemId = null;

            if (input.Type == FluidValues.Object && input.ToObjectValue() is ContentItem contentItem)
            {
                listContentItemId = contentItem.ContentItemId;
            }
            else
            {
                listContentItemId = input.ToStringValue();
            }

            var session = ((IServiceProvider)services).GetRequiredService<ISession>();

            var listCount = await ListQueryHelpers.QueryListItemsCountAsync(session, listContentItemId);

            return NumberValue.Create(listCount);
        }
    }
}
