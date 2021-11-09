using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Liquid;
using OrchardCore.Lists.Helpers;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql;

namespace OrchardCore.Lists.Liquid
{
    public class ListItemsFilter : ILiquidFilter
    {
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;

        public ListItemsFilter(ISession session, IServiceProvider serviceProvider)
        {
            _session = session;
            _serviceProvider = serviceProvider;
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
            bool useOrder = false;

            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();

            var listContentItem = await contentManager.GetAsync(listContentItemId);
            if (listContentItem != null)
            {
                var contentDefinitionManager = _serviceProvider.GetRequiredService<IContentDefinitionManager>();
                var contentTypeDefinition = contentDefinitionManager.GetTypeDefinition(listContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "ListPart"));
                var settings = contentTypePartDefinition.GetSettings<ListPartSettings>();
                if (settings.EnableOrdering)
                {
                    useOrder = true;
                }
            }

            var listItems = await ListQueryHelpers.QueryListItemsAsync(_session, listContentItemId, null, PartPredicate(useOrder));

            return FluidValue.Create(listItems, ctx.Options);
        }

        private static Expression<Func<ContainedPartIndex, object>> PartPredicate(bool useOrder)
        {
            if (useOrder)
            {
                return x => x.Order;
            }

            return null;
        }
    }
}