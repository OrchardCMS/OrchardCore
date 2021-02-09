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
    public static class ListItemsFilter
    {
        public static async ValueTask<FluidValue> ListItems(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            string listContentItemId = null;

            if (input.Type == FluidValues.Object && input.ToObjectValue() is ContentItem contentItem)
            {
                listContentItemId = contentItem.ContentItemId;
            }
            else
            {
                listContentItemId = input.ToStringValue();
            }

            var session = context.Services.GetRequiredService<ISession>();

            var listItems = await ListQueryHelpers.QueryListItemsAsync(session, listContentItemId);

            return FluidValue.Create(listItems, ctx.Options);
        }
    }
}
