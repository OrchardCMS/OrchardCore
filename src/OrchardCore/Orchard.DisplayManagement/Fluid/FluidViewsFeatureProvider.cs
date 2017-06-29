using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Options;
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

                    var matcher = new Matcher();
                    matcher.AddInclude("/**/*" + FluidViewTemplate.ViewExtension);

                    foreach (var option in expanderOptionsAccessor.Value.Options)
                    {
                        var fileInfo = fileProviderAccessor.FileProvider.GetFileInfo(option.SearchPath);
                        var directoryInfo = new DirectoryInfo(fileInfo.PhysicalPath);

                        if (directoryInfo.Exists)
                        {
                            var files = matcher.Execute(new DirectoryInfoWrapper(directoryInfo)).Files;
                            var searchPath = option.SearchPath.Replace("\\", "/").Trim('/');
                            _paths.AddRange(files.Select(f => '/' + searchPath + '/' + f.Path));
                        }
                    }
                }
            }
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            foreach (var path in _paths)
            {
                var viewPath = path.Replace(FluidViewTemplate.ViewExtension, RazorViewEngine.ViewExtension);
                feature.Views[viewPath] = typeof(FluidPage);
            }
        }
    }
}
