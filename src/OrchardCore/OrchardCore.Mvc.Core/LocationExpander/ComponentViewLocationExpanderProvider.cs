using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Mvc.FileProviders;

namespace OrchardCore.Mvc.LocationExpander;

public class ComponentViewLocationExpanderProvider : IViewLocationExpanderProvider
{
    private static readonly string s_sharedViewsPath = "/Views/Shared/{0}" + RazorViewEngine.ViewExtension;
    private static readonly string s_sharedPagesPath = "/Pages/Shared/{0}" + RazorViewEngine.ViewExtension;
    private static readonly string[] s_razorExtensions = [RazorViewEngine.ViewExtension];
    private const string CacheKey = "ModuleComponentViewLocations";
    private static List<IExtensionInfo> s_modulesWithComponentViews;
    private static List<IExtensionInfo> s_modulesWithPagesComponentViews;
    private static readonly object s_synLock = new();
    private static bool s_initialized;

    private readonly IExtensionManager _extensionManager;
    private readonly ShellDescriptor _shellDescriptor;
    private readonly IMemoryCache _memoryCache;

    public ComponentViewLocationExpanderProvider(
        RazorCompilationFileProviderAccessor fileProviderAccessor,
        IExtensionManager extensionManager,
        ShellDescriptor shellDescriptor,
        IMemoryCache memoryCache)
    {
        _extensionManager = extensionManager;
        _shellDescriptor = shellDescriptor;
        _memoryCache = memoryCache;

        if (s_initialized)
        {
            return;
        }

        lock (s_synLock)
        {
            if (!s_initialized)
            {
                var modulesWithComponentViews = new List<IExtensionInfo>();
                var modulesWithPagesComponentViews = new List<IExtensionInfo>();

                var orderedModules = _extensionManager.GetExtensions()
                    .Where(e => e.Manifest.Type.Equals("module", StringComparison.OrdinalIgnoreCase))
                    .Reverse();

                foreach (var module in orderedModules)
                {
                    var moduleComponentsViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                        module.SubPath + "/Views/Shared/Components", s_razorExtensions,
                        viewsFolder: null, inViewsFolder: true, inDepth: true);

                    if (moduleComponentsViewFilePaths.Any())
                    {
                        modulesWithComponentViews.Add(module);
                    }

                    var modulePagesComponentsViewFilePaths = fileProviderAccessor.FileProvider.GetViewFilePaths(
                        module.SubPath + "/Pages/Shared/Components", s_razorExtensions,
                        viewsFolder: null, inViewsFolder: true, inDepth: true);

                    if (modulePagesComponentsViewFilePaths.Any())
                    {
                        modulesWithPagesComponentViews.Add(module);
                    }
                }

                s_modulesWithComponentViews = modulesWithComponentViews;
                s_modulesWithPagesComponentViews = modulesWithPagesComponentViews;

                s_initialized = true;
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
            if (!_memoryCache.TryGetValue(CacheKey, out string[] moduleComponentViewLocations))
            {
                var enabledIds = _extensionManager.GetFeatures().Where(f => _shellDescriptor
                    .Features.Any(sf => sf.Id == f.Id)).Select(f => f.Extension.Id).ToHashSet();

                var enabledExtensionIds = _extensionManager
                    .GetExtensions()
                    .Where(e => enabledIds.Contains(e.Id))
                    .Select(x => x.Id)
                    .ToHashSet();

                moduleComponentViewLocations = s_modulesWithComponentViews
                    .Where(m => enabledExtensionIds.Contains(m.Id))
                    .Select(m => '/' + m.SubPath + s_sharedViewsPath)
                    .Concat(s_modulesWithPagesComponentViews
                    .Where(m => enabledExtensionIds.Contains(m.Id))
                    .Select(m => '/' + m.SubPath + s_sharedPagesPath))
                    .ToArray();

                _memoryCache.Set(CacheKey, moduleComponentViewLocations);
            }

            result.AddRange(moduleComponentViewLocations);
        }

        result.AddRange(viewLocations);

        return result;
    }
}
