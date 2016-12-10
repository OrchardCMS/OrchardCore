using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell;
using System.Linq;
using System.IO;
using Orchard.Environment.Extensions;

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

            var extensionsManager = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IExtensionManager>();

            // Get Extension, and then add in the relevant views.
            var extension = extensionsManager.GetExtension(context.AreaName);

            if (extension != null)
            {
                var extensionViewsPath = 
                    Path.Combine(Path.DirectorySeparatorChar + extension.SubPath, "Views");

                result.Add(Path.Combine(extensionViewsPath, "{1}", "{0}.cshtml"));
                result.Add(Path.Combine(extensionViewsPath, "Shared", "{0}.cshtml"));
            }

            result.AddRange(viewLocations);

            return result;
        }
    }
}