using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.Modules.LocationExpander
{
    internal class CompositeViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        public double Priority
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var expanderProviders = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetServices<IViewLocationExpanderProvider>();

            var locations = new List<string>();

            foreach (var provider in expanderProviders.OrderByDescending(x => x.Priority))
            {
                var entry = provider.ExpandViewLocations(context, viewLocations);
                if (entry != null)
                {
                    locations.AddRange(entry);
                }
            }

            return locations.Distinct();
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var expanderProviders = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetServices<IViewLocationExpanderProvider>();

            foreach (var provider in expanderProviders.OrderByDescending(x => x.Priority))
            {
                provider.PopulateValues(context);
            }
        }
    }
}
