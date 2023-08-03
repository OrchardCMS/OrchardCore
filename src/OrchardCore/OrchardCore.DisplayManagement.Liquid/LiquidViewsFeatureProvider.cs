using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fluid;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.Modules;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        public const string DefaultLiquidViewName = "DefaultLiquidViewName";
        public static readonly string DefaultRazorViewPath = '/' + DefaultLiquidViewName + RazorViewEngine.ViewExtension;
        public static readonly string DefaultLiquidViewPath = '/' + DefaultLiquidViewName + LiquidViewTemplate.ViewExtension;

        private static List<string> _sharedPaths;
        private static readonly object _synLock = new();

        public LiquidViewsFeatureProvider(IOptions<TemplateOptions> templateOptions)
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

                    var filePaths = templateOptions.Value.FileProvider.GetViewFilePaths(
                        Application.ModulesPath, new[] { LiquidViewTemplate.ViewExtension },
                        LiquidViewTemplate.ViewsFolder);

                    _sharedPaths.AddRange(filePaths.Select(p => '/' + p));
                }
            }
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            feature.ViewDescriptors.Add(new CompiledViewDescriptor
            {
                RelativePath = DefaultRazorViewPath,
                Item = new RazorViewCompiledItem(typeof(LiquidPage), @"mvc.1.0.view", DefaultLiquidViewPath)
            });

            foreach (var path in _sharedPaths)
            {
                if (!Path.GetFileName(path).StartsWith('_'))
                {
                    var viewPath = Path.ChangeExtension(path, RazorViewEngine.ViewExtension);
                    feature.ViewDescriptors.Add(new CompiledViewDescriptor
                    {
                        RelativePath = viewPath,
                        Item = new RazorViewCompiledItem(typeof(LiquidPage), @"mvc.1.0.view", viewPath)
                    });
                }
            }
        }
    }
}
