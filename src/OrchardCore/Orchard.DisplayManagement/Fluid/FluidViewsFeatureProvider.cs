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
        private static List<string> _sharedPaths;
        private static object _synLock = new object();

        public FluidViewsFeatureProvider(
            IFluidViewFileProviderAccessor fileProviderAccessor,
            IOptions<ExtensionExpanderOptions> expanderOptionsAccessor)
        {
            if (_sharedPaths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_sharedPaths == null)
                {
                    _sharedPaths = new List<string>();

                    foreach (var option in expanderOptionsAccessor.Value.Options)
                    {
                        var filePaths = fileProviderAccessor.SharedFileProvider.GetViewFilePaths(
                            option.SearchPath, new[] { FluidViewTemplate.ViewExtension },
                            FluidViewTemplate.ViewsFolder);

                        _sharedPaths.AddRange(filePaths.Select(p => string.Concat('/', p)));
                    }
                }
            }
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            foreach (var path in _sharedPaths)
            {
                if (!Path.GetFileName(path).StartsWith("_"))
                {
                    var viewPath = Path.ChangeExtension(path, RazorViewEngine.ViewExtension);
                    feature.Views[viewPath] = typeof(FluidPage);
                }
            }
        }
    }
}
