using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.Mvc.LocationExpander;

public class SharedViewLocationExpanderProvider : IViewLocationExpanderProvider
{
    private static readonly string s_pageSharedViewsPath = "/Pages/Shared/{0}" + RazorViewEngine.ViewExtension;
    private static readonly string s_sharedViewsPath = "/Views/Shared/{0}" + RazorViewEngine.ViewExtension;

    private static readonly string[] s_razorExtensions = [RazorViewEngine.ViewExtension];
    private const string CacheKey = "ModuleSharedViewLocations";
    private const string PageCacheKey = "ModulePageSharedViewLocations";
    private static List<IExtensionInfo> s_modulesWithPageSharedViews;
    private static List<IExtensionInfo> s_modulesWithSharedViews;
    private static readonly object s_synLock = new();

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

        if (s_modulesWithSharedViews != null)
        {
            return;
        }

        lock (s_synLock)
        {
            if (s_modulesWithSharedViews == null)
            {
                var orderedModules = _extensionManager.GetExtensions()
                    .Where(e => e.Manifest.Type.Equals("module", StringComparison.OrdinalIgnoreCase))
                    .Reverse();

                var modulesWithPageSharedViews = new List<IExtensionInfo>();
                var modulesWithSharedViews = new List<IExtensionInfo>();

                foreach (var module in orderedModules)
                {
                    var modulePageSharedViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                        module.SubPath + "/Pages/Shared", s_razorExtensions,
                        viewsFolder: null, inViewsFolder: true, inDepth: true);

                    if (modulePageSharedViewFilePaths.Any())
                    {
                        modulesWithPageSharedViews.Add(module);
                    }

                    var moduleSharedViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                        module.SubPath + "/Views/Shared", s_razorExtensions,
                        viewsFolder: null, inViewsFolder: true, inDepth: true);

                    if (moduleSharedViewFilePaths.Any())
                    {
                        modulesWithSharedViews.Add(module);
                    }
                }

                s_modulesWithPageSharedViews = modulesWithPageSharedViews;
                s_modulesWithSharedViews = modulesWithSharedViews;
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
                modulePageSharedViewLocations = s_modulesWithPageSharedViews
                    .Where(m => GetEnabledExtensionIds().Contains(m.Id))
                    .Select(m => '/' + m.SubPath + s_pageSharedViewsPath);

                _memoryCache.Set(PageCacheKey, modulePageSharedViewLocations);
            }

            result.AddRange(modulePageSharedViewLocations);
        }

        if (!_memoryCache.TryGetValue(CacheKey, out IEnumerable<string> moduleSharedViewLocations))
        {
            moduleSharedViewLocations = s_modulesWithSharedViews
                .Where(m => GetEnabledExtensionIds().Contains(m.Id))
                .Select(m => '/' + m.SubPath + s_sharedViewsPath);

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
