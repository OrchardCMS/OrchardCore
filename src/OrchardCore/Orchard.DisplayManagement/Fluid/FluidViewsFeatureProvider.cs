using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.FileProviders;
using Orchard.DisplayManagement.Fluid.Internal;
using Orchard.Environment.Extensions;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        private static List<string> _paths;
        private static object _synLock = new object();

        public FluidViewsFeatureProvider(
            IFluidViewFileProviderAccessor fileProviderAccessor,
            IOptions<ExtensionExpanderOptions> expanderOptionsAccessor)
        {
            if (_paths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_paths == null)
                {
                    _paths = new List<string>();

                    foreach (var option in expanderOptionsAccessor.Value.Options)
                    {
                        var searchPath = option.SearchPath.Replace("\\", "/").Trim('/');
                        var filePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                            searchPath, new[] { FluidViewTemplate.ViewExtension });
                        _paths.AddRange(filePaths.Select(p => string.Format("/{0}", p)));
                    }
                }
            }
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            foreach (var path in _paths)
            {
                if (!Path.GetFileName(path).StartsWith("_"))
                {
                    var viewPath = path.Replace(FluidViewTemplate.ViewExtension, RazorViewEngine.ViewExtension);
                    feature.Views[viewPath] = typeof(FluidPage);
                }
            }
        }
    }
}
