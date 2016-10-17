using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Orchard.Hosting.Mvc.Razor
{
    public class ModuleViewLocationExpander : IViewLocationExpander
    {
        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            var result = new List<string>();

            result.Add("/Modules/{2}/Views/{1}/{0}.cshtml");
            result.Add("/Modules/{2}/Views/Shared/{0}.cshtml");

            result.AddRange(viewLocations);

            return result;
        }
    }
}