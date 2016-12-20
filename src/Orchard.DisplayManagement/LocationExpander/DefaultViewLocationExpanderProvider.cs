using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Orchard.DisplayManagement.LocationExpander
{
    public class DefaultViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        public double Priority => 0D;

        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            return viewLocations;
        }
    }
}