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
using Orchard.Mvc;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewsFeatureProvider : IShellFeatureProvider<ViewsFeature>
    {
        private static List<string> _SharedPaths;
        private static object _synLock = new object();
        private readonly List<string> _shellPaths;

        public FluidViewsFeatureProvider(
            IFluidViewFileProviderAccessor fileProviderAccessor,
            IOptions<ExtensionExpanderOptions> expanderOptionsAccessor)
        {
            _shellPaths = new List<string>();

            foreach (var option in expanderOptionsAccessor.Value.Options)
            {
                var filePaths = fileProviderAccessor.ShellFileProvider.GetViewFilePaths(
                    option.SearchPath, new[] { FluidViewTemplate.ViewExtension });
                _shellPaths.AddRange(filePaths.Select(p => string.Format("/{0}", p)));
            }

            if (_SharedPaths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_SharedPaths == null)
                {
                    _SharedPaths = new List<string>();

                    foreach (var option in expanderOptionsAccessor.Value.Options)
                    {
                        var filePaths = fileProviderAccessor.SharedFileProvider.GetViewFilePaths(
                            option.SearchPath, new[] { FluidViewTemplate.ViewExtension });
                        _SharedPaths.AddRange(filePaths.Select(p => string.Format("/{0}", p)));
                    }
                }
            }
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            foreach (var path in _SharedPaths)
            {
                if (!Path.GetFileName(path).StartsWith("_"))
                {
                    var viewPath = Path.ChangeExtension(path, RazorViewEngine.ViewExtension);
                    feature.Views[viewPath] = typeof(FluidPage);
                }
            }
        }

        public void PopulateShellFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            foreach (var path in _shellPaths)
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
