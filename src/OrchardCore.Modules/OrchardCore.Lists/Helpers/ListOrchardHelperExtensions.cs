using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Lists.Helpers
{
    public static class ListOrchardHelperExtensions
    {
        public static async Task<int> QueryListItemsCountAsync(this IOrchardHelper orchardHelper, string listContentItemId, Expression<Func<ContentItemIndex, bool>> itemPredicate = null)
        {
            var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();

            return await ListQueryHelpers
                .QueryListItemsCountAsync(session, listContentItemId, itemPredicate ?? ListQueryHelpers.CreateContentIndexFilter(null, null, true));
        }

        public static async Task<IEnumerable<ContentItem>> QueryListItemsAsync(this IOrchardHelper orchardHelper, string listContentItemId, Expression<Func<ContentItemIndex, bool>> itemPredicate = null)
        {
            var session = orchardHelper.HttpContext.RequestServices.GetService<ISession>();

            return await ListQueryHelpers
                .QueryListItemsAsync(session, listContentItemId, itemPredicate ?? ListQueryHelpers.CreateContentIndexFilter(null, null, true));
        }
    }
}
