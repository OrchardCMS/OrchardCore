using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Hosting.Mvc.Filters
{
    public class DependencyFilterProvider : IFilterProvider
    {
        public int Order
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public void OnProvidersExecuting(FilterProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var services = context.ActionContext.HttpContext.RequestServices;
            var filters = services.GetService<IEnumerable<IFilterMetadata>>();

            if (context.ActionContext.ActionDescriptor.FilterDescriptors != null)
            {
                foreach (var filter in filters)
                {
                    var filterItem = new FilterItem(new FilterDescriptor(filter, FilterScope.Global), filter);
                    context.Results.Add(filterItem);
                }
            }
        }

        /// <inheritdoc />
        public void OnProvidersExecuted(FilterProviderContext context)
        {
        }
    }
}
