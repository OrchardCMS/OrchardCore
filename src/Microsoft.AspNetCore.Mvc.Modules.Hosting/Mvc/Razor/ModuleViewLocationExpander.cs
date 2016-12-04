using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Orchard.Environment.Extensions;

namespace Orchard.Hosting.Mvc.Razor
{
    public class ModuleViewLocationExpander : IViewLocationExpander
    {
        private readonly IEnumerable<IExtensionInfo> _extensions;

        public ModuleViewLocationExpander() : this(new ExtensionInfoList(new List<IExtensionInfo>()))
        {
        }

        public ModuleViewLocationExpander(IExtensionInfoList extensions)
        {
            _extensions = extensions.Features.Where(x => x.Id == x.Extension.Id)
                .Select(x => x.Extension).Reverse();
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

            foreach (var extension in _extensions)
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