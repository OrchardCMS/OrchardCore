using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Microsoft.AspNetCore.Mvc.Modules.LocationExpander
{
    public class DefaultViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        public int Priority => 0;

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