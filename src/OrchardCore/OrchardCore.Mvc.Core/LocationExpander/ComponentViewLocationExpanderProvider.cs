using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.Mvc.LocationExpander
{
    public class ComponentViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private const string CacheKey = "ModuleComponentViewLocations)";
        private static IList<IExtensionInfo> _modulesWithComponentViews;
        private static object _synLock = new object();

        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IMemoryCache _memoryCache;

        public ComponentViewLocationExpanderProvider(
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
                    var modulesWithComponentViews = new List<IExtensionInfo>();

                    var orderedModules = _extensionManager.GetExtensions()
                        .Where(e => e.Manifest.Type.Equals("module", StringComparison.OrdinalIgnoreCase))
                        .Reverse();

                    foreach (var module in orderedModules)
                    {
                        var moduleComponentsViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                            module.SubPath + "/Views/Shared/Components", new[] { RazorViewEngine.ViewExtension },
                            viewsFolder: null, inViewsFolder: true, inDepth: true);

                        if (moduleComponentsViewFilePaths.Any())
                        {
                            modulesWithComponentViews.Add(module);
                        }
                    }

                    _modulesWithComponentViews = modulesWithComponentViews;
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
            if (context.AreaName == null)
            {
                return viewLocations;
            }

            var result = new List<string>();

            if (context.ViewName.StartsWith("Components/", StringComparison.Ordinal))
            {
                if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<string> moduleComponentViewLocations))
                {
                    var enabledIds = _extensionManager.GetFeatures().Where(f => _shellDescriptor
                        .Features.Any(sf => sf.Id == f.Id)).Select(f => f.Extension.Id).Distinct().ToArray();

                    var enabledExtensions = _extensionManager.GetExtensions()
                        .Where(e => enabledIds.Contains(e.Id)).ToArray();

                    var sharedViewsPath = "/Views/Shared/{0}" + RazorViewEngine.ViewExtension;

                    moduleComponentViewLocations = _modulesWithComponentViews
                        .Where(m => enabledExtensions.Any(e => e.Id == m.Id))
                        .Select(m => '/' + m.SubPath + sharedViewsPath);

                    _memoryCache.Set(CacheKey, moduleComponentViewLocations);
                }

                result.AddRange(moduleComponentViewLocations);
            }

            result.AddRange(viewLocations);

            return result;
        }
    }
}
