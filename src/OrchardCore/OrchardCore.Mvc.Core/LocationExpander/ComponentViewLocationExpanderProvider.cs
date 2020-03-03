using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.Mvc.LocationExpander
{
    public class ComponentViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private static readonly string SharedViewsPath = "/Views/Shared/{0}" + RazorViewEngine.ViewExtension;
        private static readonly string SharedPagesPath = "/Pages/Shared/{0}" + RazorViewEngine.ViewExtension;
        private static readonly string[] RazorExtensions = new[] { RazorViewEngine.ViewExtension };
        private const string CacheKey = "ModuleComponentViewLocations";
        private static List<IExtensionInfo> _modulesWithComponentViews;
        private static List<IExtensionInfo> _modulesWithPagesComponentViews;
        private static object _synLock = new object();

        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IMemoryCache _memoryCache;

        private bool _initialized;

        public ComponentViewLocationExpanderProvider(
            RazorCompilationFileProviderAccessor fileProviderAccessor,
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IMemoryCache memoryCache)
        {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _memoryCache = memoryCache;

            if (_modulesWithComponentViews != null && _modulesWithPagesComponentViews != null)
            {
                return;
            }

            lock (_synLock)
            {
                var orderedModules = _extensionManager.GetExtensions()
                        .Where(e => e.Manifest.Type.Equals("module", StringComparison.OrdinalIgnoreCase))
                        .Reverse();
                if (_modulesWithComponentViews == null)
                {
                    var modulesWithComponentViews = new List<IExtensionInfo>();

                    foreach (var module in orderedModules)
                    {
                        var moduleComponentsViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                            module.SubPath + "/Views/Shared/Components", RazorExtensions,
                            viewsFolder: null, inViewsFolder: true, inDepth: true);

                        if (moduleComponentsViewFilePaths.Any())
                        {
                            modulesWithComponentViews.Add(module);
                        }
                    }

                    _modulesWithComponentViews = modulesWithComponentViews;
                }

                if (_modulesWithPagesComponentViews == null)
                {
                    var modulesWithPagesComponentViews = new List<IExtensionInfo>();

                    foreach (var module in orderedModules)
                    {
                        var moduleWithPagesComponentsViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                            module.SubPath + "/Pages/Shared/Components", RazorExtensions,
                            viewsFolder: null, inViewsFolder: true, inDepth: true);

                        if (moduleWithPagesComponentsViewFilePaths.Any())
                        {
                            modulesWithPagesComponentViews.Add(module);
                        }
                    }

                    _modulesWithPagesComponentViews = modulesWithPagesComponentViews;
                }
            }

            if (_modulesWithComponentViews != null && _modulesWithPagesComponentViews != null)
            {
                _initialized = true;
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

            if (context.ViewName.StartsWith("Components/", StringComparison.Ordinal) && _initialized)
            {
                var enabledIds = _extensionManager
                    .GetFeatures()
                    .Where(f => _shellDescriptor
                    .Features.Any(sf => sf.Id == f.Id)).Select(f => f.Extension.Id).ToHashSet();
                var enabledExtensionIds = _extensionManager
                    .GetExtensions()
                    .Where(e => enabledIds.Contains(e.Id))
                    .Select(x => x.Id)
                    .ToHashSet();

                if (!_memoryCache.TryGetValue(CacheKey, out string[] moduleComponentViewLocations))
                {
                    moduleComponentViewLocations = _modulesWithComponentViews
                        .Where(m => enabledExtensionIds.Contains(m.Id))
                        .Select(m => '/' + m.SubPath + SharedViewsPath)
                        .ToArray();

                    _memoryCache.Set(CacheKey, moduleComponentViewLocations);
                }

                result.AddRange(moduleComponentViewLocations);

                if (!_memoryCache.TryGetValue(CacheKey, out string[] moduleWithPagesComponentViewLocations))
                {
                    moduleWithPagesComponentViewLocations = _modulesWithPagesComponentViews
                        .Where(m => enabledExtensionIds.Contains(m.Id))
                        .Select(m => '/' + m.SubPath + SharedPagesPath)
                        .ToArray();

                    _memoryCache.Set(CacheKey, moduleWithPagesComponentViewLocations);
                }

                result.AddRange(moduleWithPagesComponentViewLocations);
            }

            result.AddRange(viewLocations);

            return result;
        }
    }
}
