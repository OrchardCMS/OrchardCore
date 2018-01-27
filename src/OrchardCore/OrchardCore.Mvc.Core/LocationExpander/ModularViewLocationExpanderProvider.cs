using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.Mvc.LocationExpander
{
    public class ModularViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private const string CacheKey = "ModuleComponentViewLocations)";
        private static IList<IExtensionInfo> _modulesWithComponentViews;
        private static object _synLock = new object();

        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IMemoryCache _memoryCache;

        public ModularViewLocationExpanderProvider(
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IMemoryCache memoryCache)
        {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _memoryCache = memoryCache;

            if (_modulesWithComponentViews != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_modulesWithComponentViews == null)
                {
                    _modulesWithComponentViews = new List<IExtensionInfo>();

                    var modules = _extensionManager.GetExtensions()
                        .Where(e => e.Manifest?.Type?.Equals("module", StringComparison.OrdinalIgnoreCase) ?? false);

                    foreach (var module in modules)
                    {
                        var moduleComponentsViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                            module.SubPath + "/Views/Shared/Components", new[] { RazorViewEngine.ViewExtension },
                            viewsFolder: null, inViewsFolder: true, inDepth: true);

                        if (moduleComponentsViewFilePaths.Any())
                        {
                            _modulesWithComponentViews.Add(module);
                        }
                    }
                }
            }
        }

        public int Priority => 5;

        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
            if (context.ActionContext.ActionDescriptor is PageActionDescriptor page)
            {
                var pageViewLocations = PageViewLocations().ToList();
                pageViewLocations.AddRange(viewLocations);
                return pageViewLocations;

                IEnumerable<string> PageViewLocations()
                {
                    if (page.RelativePath.Contains("/Pages/") && !page.RelativePath.StartsWith("/Pages/", StringComparison.Ordinal))
                    {
                        yield return page.RelativePath.Substring(0, page.RelativePath.IndexOf("/Pages/", StringComparison.Ordinal))
                            + "/Views/Shared/{0}" + RazorViewEngine.ViewExtension;
                    }
                }
            }

            // Get Extension, and then add in the relevant views.
            var extension = _extensionManager.GetExtension(context.AreaName);

            if (!extension.Exists)
            {
                return viewLocations;
            }

            var result = new List<string>();

            var extensionViewsPath = '/' + extension.SubPath + "/Views";
            result.Add(extensionViewsPath + "/{1}/{0}" + RazorViewEngine.ViewExtension);

            if (!context.ViewName.StartsWith("Components/", StringComparison.Ordinal))
            {
                result.Add(extensionViewsPath + "/Shared/{0}" + RazorViewEngine.ViewExtension);
            }
            else
            {
                if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<string> moduleComponentViewLocations))
                {
                    moduleComponentViewLocations = _extensionManager.GetFeatures()
                        .Where(f => f.Id == f.Extension.Id &&
                            _modulesWithComponentViews.Any(m => m.Id == f.Id) &&
                            _shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                        .Select(f => '/' + f.Extension.SubPath + "/Views" + "/Shared/{0}" + RazorViewEngine.ViewExtension)
                        .Reverse();

                    _memoryCache.Set(CacheKey, moduleComponentViewLocations);
                }

                result.AddRange(moduleComponentViewLocations);
            }

            result.AddRange(viewLocations);

            return result;
        }
    }
}
