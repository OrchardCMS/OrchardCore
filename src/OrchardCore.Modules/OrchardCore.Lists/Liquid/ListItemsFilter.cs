using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Lists.Helpers;
using YesSql;

namespace OrchardCore.Lists.Liquid
{
    public class ListItemsFilter : ILiquidFilter
    {
        private readonly ISession _session;

        public ListItemsFilter(ISession session)
        {
            _session = session;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            string listContentItemId;

            if (input.Type == FluidValues.Object && input.ToObjectValue() is ContentItem contentItem)
            {
                listContentItemId = contentItem.ContentItemId;
            }
            else
            {
                listContentItemId = input.ToStringValue();
            }

            var listItems = await ListQueryHelpers.QueryListItemsAsync(_session, listContentItemId);

            return FluidValue.Create(listItems, ctx.Options);
        }
    }
}
