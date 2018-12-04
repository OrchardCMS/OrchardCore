using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using OrchardCore.DisplayManagement.Liquid.Internal;
using OrchardCore.Modules;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        public const string DefaultLiquidViewName = "DefaultLiquidViewName";
        public static string DefaultRazorViewPath = '/' + DefaultLiquidViewName + RazorViewEngine.ViewExtension;
        public static string DefaultLiquidViewPath = '/' + DefaultLiquidViewName + LiquidViewTemplate.ViewExtension;

        private static List<string> _sharedPaths;
        private static object _synLock = new object();
        private readonly IHostingEnvironment _hostingEnvironment;

        public LiquidViewsFeatureProvider(
            IHostingEnvironment hostingEnvironment,
            ILiquidViewFileProviderAccessor fileProviderAccessor)
        {
            _hostingEnvironment = hostingEnvironment;

            if (_sharedPaths != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_sharedPaths == null)
                {
                    _sharedPaths = new List<string>();

                    var filePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                        Application.ModulesPath, new[] { LiquidViewTemplate.ViewExtension },
                        LiquidViewTemplate.ViewsFolder);

                    _sharedPaths.AddRange(filePaths.Select(p => '/' + p));
                }
            }
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            if (!parts.Where(p => p.Name == _hostingEnvironment.ApplicationName).Any())
            {
                return;
            }

            feature.ViewDescriptors.Add(new CompiledViewDescriptor
            {
                RelativePath = DefaultRazorViewPath,
                ViewAttribute = new RazorViewAttribute(DefaultLiquidViewPath, typeof(LiquidPage))
            });

            foreach (var path in _sharedPaths)
            {
                if (!Path.GetFileName(path).StartsWith("_"))
                {
                    var viewPath = Path.ChangeExtension(path, RazorViewEngine.ViewExtension);
                    feature.ViewDescriptors.Add(new CompiledViewDescriptor { RelativePath = viewPath, ViewAttribute = new RazorViewAttribute(path, typeof(LiquidPage)) });
                }
            }
        }
    }
}