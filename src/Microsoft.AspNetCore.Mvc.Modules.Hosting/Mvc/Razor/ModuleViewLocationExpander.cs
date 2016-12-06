using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell;

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

            var extensionManager = context.ActionContext.HttpContext.RequestServices.GetService<IExtensionManager>();
            var shellFeaturesManager = context.ActionContext.HttpContext.RequestServices.GetService<IShellFeaturesManager>();

            var availableFeatures = extensionManager.GetExtensions().Features;
            var enabledFeatures = shellFeaturesManager.GetEnabledFeaturesAsync().Result;

            var features = availableFeatures.Where(af => enabledFeatures.Any(ef => af.Id == ef.Id));
            var extensions = features.Where(f => f.Id == f.Extension.Id).Select(f => f.Extension).Reverse();

            foreach (var extension in extensions)
            {
                var viewsPath = Path.Combine(Path.DirectorySeparatorChar + extension.SubPath,
                    "Views", context.AreaName != extension.Id ? context.AreaName : String.Empty);

                result.Add(Path.Combine(viewsPath, "{1}", "{0}.cshtml"));
                result.Add(Path.Combine(viewsPath, "Shared", "{0}.cshtml"));
            }

            result.AddRange(viewLocations);

            return result;
        }
    }
}