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
    public class SharedViewLocationExpanderProvider : IViewLocationExpanderProvider
    {
        private static readonly string _pageSharedViewsPath = "/Pages/Shared/{0}" + RazorViewEngine.ViewExtension;
        private static readonly string _sharedViewsPath = "/Views/Shared/{0}" + RazorViewEngine.ViewExtension;

        private static readonly string[] _razorExtensions = new[] { RazorViewEngine.ViewExtension };
        private const string CacheKey = "ModuleSharedViewLocations";
        private const string PageCacheKey = "ModulePageSharedViewLocations";
        private static List<IExtensionInfo> _modulesWithPageSharedViews;
        private static List<IExtensionInfo> _modulesWithSharedViews;
        private static readonly object _synLock = new();

        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IMemoryCache _memoryCache;

        public SharedViewLocationExpanderProvider(
            RazorCompilationFileProviderAccessor fileProviderAccessor,
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IMemoryCache memoryCache)
        {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _memoryCache = memoryCache;

            if (_modulesWithSharedViews != null)
            {
                return;
            }

            lock (_synLock)
            {
                if (_modulesWithSharedViews == null)
                {
                    var orderedModules = _extensionManager.GetExtensions()
                        .Where(e => e.Manifest.Type.Equals("module", StringComparison.OrdinalIgnoreCase))
                        .Reverse();

                    var modulesWithPageSharedViews = new List<IExtensionInfo>();
                    var modulesWithSharedViews = new List<IExtensionInfo>();

                    foreach (var module in orderedModules)
                    {
                        var modulePageSharedViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                            module.SubPath + "/Pages/Shared", _razorExtensions,
                            viewsFolder: null, inViewsFolder: true, inDepth: true);

                        if (modulePageSharedViewFilePaths.Any())
                        {
                            modulesWithPageSharedViews.Add(module);
                        }

                        var moduleSharedViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                            module.SubPath + "/Views/Shared", _razorExtensions,
                            viewsFolder: null, inViewsFolder: true, inDepth: true);

                        if (moduleSharedViewFilePaths.Any())
                        {
                            modulesWithSharedViews.Add(module);
                        }
                    }

                    _modulesWithPageSharedViews = modulesWithPageSharedViews;
                    _modulesWithSharedViews = modulesWithSharedViews;
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

            HashSet<string> enabledExtensionIds = null;

            var result = new List<string>();

            if (context.PageName != null)
            {
                if (!_memoryCache.TryGetValue(PageCacheKey, out IEnumerable<string> modulePageSharedViewLocations))
                {
                    modulePageSharedViewLocations = _modulesWithPageSharedViews
                        .Where(m => GetEnabledExtensionIds().Contains(m.Id))
                        .Select(m => '/' + m.SubPath + _pageSharedViewsPath);

                    _memoryCache.Set(PageCacheKey, modulePageSharedViewLocations);
                }

                result.AddRange(modulePageSharedViewLocations);
            }

            if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<string> moduleSharedViewLocations))
            {
                moduleSharedViewLocations = _modulesWithSharedViews
                    .Where(m => GetEnabledExtensionIds().Contains(m.Id))
                    .Select(m => '/' + m.SubPath + _sharedViewsPath);

                _memoryCache.Set(CacheKey, moduleSharedViewLocations);
            }

            result.AddRange(moduleSharedViewLocations);
            result.AddRange(viewLocations);

            return result;

            HashSet<string> GetEnabledExtensionIds()
            {
                if (enabledExtensionIds != null)
                {
                    return enabledExtensionIds;
                }

                var enabledIds = _extensionManager.GetFeatures().Where(f => _shellDescriptor
                        .Features.Any(sf => sf.Id == f.Id)).Select(f => f.Extension.Id).ToHashSet();

                return enabledExtensionIds = _extensionManager.GetExtensions()
                    .Where(e => enabledIds.Contains(e.Id)).Select(x => x.Id).ToHashSet();
            }
        }
    }
}
