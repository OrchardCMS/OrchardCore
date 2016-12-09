using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Orchard.Hosting.Mvc.Razor
{
    public class ModuleViewLocationExpander : IViewLocationExpander
    {
        private string[] _moduleIds;

        public ModuleViewLocationExpander() : this(Array.Empty<string>())
        {
        }

        public ModuleViewLocationExpander(string[] moduleIds)
        {
            _moduleIds = moduleIds;
        }

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

            // TODO: This is a dumb implementation in order to find the ViewComponent views when
            // it's called from a different module than the current controller.
            // This might also be enhanced to add support for Themes to override module controllers.
            
            foreach (var moduleId in _moduleIds)
            {
                result.Add("/Modules/" + moduleId + "/Views/{1}/{0}.cshtml");
                result.Add("/Modules/" + moduleId + "/Views/Shared/{0}.cshtml");
            }

            return result;
        }
    }
}