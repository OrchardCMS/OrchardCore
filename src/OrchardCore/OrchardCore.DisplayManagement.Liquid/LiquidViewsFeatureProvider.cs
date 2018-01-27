using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid.Internal;
using OrchardCore.Environment.Extensions;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        private static List<string> _sharedPaths;
        private static object _synLock = new object();

        public LiquidViewsFeatureProvider(
            ILiquidViewFileProviderAccessor fileProviderAccessor,
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
                        var filePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                            option.SearchPath, new[] { LiquidViewTemplate.ViewExtension },
                            LiquidViewTemplate.ViewsFolder);

                        _sharedPaths.AddRange(filePaths.Select(p => '/' + p));
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
                    feature.ViewDescriptors.Add( new CompiledViewDescriptor { RelativePath = viewPath, ViewAttribute = new RazorViewAttribute(path, typeof(LiquidPage)) });
                }
            }
        }
    }
}
