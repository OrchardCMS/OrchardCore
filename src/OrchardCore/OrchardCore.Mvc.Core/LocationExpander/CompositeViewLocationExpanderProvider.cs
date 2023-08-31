using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Mvc.LocationExpander
{
    internal class CompositeViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        public int Priority
        {
            get { throw new NotSupportedException(); }
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var expanderProviders = DiscoverProviders(context);

            foreach (var provider in expanderProviders.OrderBy(x => x.Priority))
            {
                viewLocations = provider.ExpandViewLocations(context, viewLocations);
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var expanderProviders = DiscoverProviders(context);

            foreach (var provider in expanderProviders.OrderBy(x => x.Priority))
            {
                provider.PopulateValues(context);
            }
        }

        private static IEnumerable<IViewLocationExpanderProvider> DiscoverProviders(ViewLocationExpanderContext context)
        {
            return context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetServices<IViewLocationExpanderProvider>();
        }
    }
}
