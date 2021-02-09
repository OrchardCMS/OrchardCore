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
    public static class ListCountFilter
    {
        public static async ValueTask<FluidValue> ListCount(FluidValue input, FilterArguments arguments, TemplateContext ctx)
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

            var listCount = await ListQueryHelpers.QueryListItemsCountAsync(session, listContentItemId);

            return NumberValue.Create(listCount);
        }
    }
}
