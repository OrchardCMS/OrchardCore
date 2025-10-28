using System.Collections.Concurrent;
using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking;

namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// This class needs to use a cache which is a singleton per tenant as it can contain different shapes
/// for each tenant, even if they share the same theme.
/// </summary>
public class DefaultShapeTableManager : IShapeTableManager
{
    private const string DefaultThemeIdKey = "_ShapeTable";

    // FeatureShapeDescriptors are identical across tenants so they can be reused statically. Each shape table will
    // create a unique list of these per tenant.
    private static readonly ConcurrentDictionary<string, FeatureShapeDescriptor> _shapeDescriptors = new();

    private static readonly object _syncLock = new();

    // Singleton cache to hold a tenant's theme ShapeTable.
    private readonly IDictionary<string, Task<ShapeTable>> _shapeTableCache;

    private readonly IServiceProvider _serviceProvider;

    public DefaultShapeTableManager(
        [FromKeyedServices(nameof(DefaultShapeTableManager))] IDictionary<string, Task<ShapeTable>> shapeTableCache,
        IServiceProvider serviceProvider)
    {
        _shapeTableCache = shapeTableCache;
        _serviceProvider = serviceProvider;
    }

    public Task<ShapeTable> GetShapeTableAsync(string themeId)
    {
        // This method is intentionally not awaited since most calls
        // are from cache.
        if (_shapeTableCache.TryGetValue(themeId ?? DefaultThemeIdKey, out var shapeTable))
        {
            return shapeTable;
        }

        return Awaited(themeId);

        async Task<ShapeTable> Awaited(string themeId)
        {
            // Use a local lock to avoid multiple concurrent builds of the same shape table. Using the ILocalLock
            // avoids holding on to a semaphore for the whole application lifetime.
            var localLock = _serviceProvider.GetRequiredService<ILocalLock>();

            using var locker = await localLock.AcquireLockAsync(nameof(DefaultShapeTableManager));

            if (_shapeTableCache.TryGetValue(themeId ?? DefaultThemeIdKey, out shapeTable))
            {
                return shapeTable.Result;
            }

            return await BuildShapeTableAsync(themeId);
        }
    }

    private async Task<ShapeTable> BuildShapeTableAsync(string themeId)
    {
        // These services are resolved lazily since they are only required when initializing the shape tables
        // during the first request. And binding strategies would be expensive to build since this service is called many times
        // per request.

        var logger = _serviceProvider.GetRequiredService<ILogger<DefaultShapeTableManager>>();
        var hostingEnvironment = _serviceProvider.GetRequiredService<IHostEnvironment>();
        var bindingStrategies = _serviceProvider.GetRequiredService<IEnumerable<IShapeTableProvider>>();
        var shellFeaturesManager = _serviceProvider.GetRequiredService<IShellFeaturesManager>();
        var extensionManager = _serviceProvider.GetRequiredService<IExtensionManager>();
        var typeFeatureProvider = _serviceProvider.GetRequiredService<ITypeFeatureProvider>();

        logger.LogInformation("Start building shape table for {Theme}", themeId);

        HashSet<string> excludedFeatures;

        // Here we don't use a lock for thread safety but for atomicity.
        lock (_syncLock)
        {
            excludedFeatures = new HashSet<string>(_shapeDescriptors.Select(kv => kv.Value.Feature.Id));
        }

        var shapeDescriptors = new Dictionary<string, FeatureShapeDescriptor>();

        foreach (var bindingStrategy in bindingStrategies)
        {
            var strategyFeature = typeFeatureProvider.GetFeatureForDependency(bindingStrategy.GetType());

            var builder = new ShapeTableBuilder(strategyFeature, excludedFeatures);
            await bindingStrategy.DiscoverAsync(builder);
            var builtAlterations = builder.BuildAlterations();

            BuildDescriptors(bindingStrategy, builtAlterations, shapeDescriptors);
        }

        // Here we don't use a lock for thread safety but for atomicity.
        lock (_syncLock)
        {
            foreach (var kv in shapeDescriptors)
            {
                _shapeDescriptors[kv.Key] = kv.Value;
            }
        }

        var enabledAndOrderedFeatureIds = (await shellFeaturesManager.GetEnabledFeaturesAsync())
            .Select(f => f.Id)
            .ToList();

        // let the application acting as a super theme for shapes rendering.
        if (enabledAndOrderedFeatureIds.Remove(hostingEnvironment.ApplicationName))
        {
            enabledAndOrderedFeatureIds.Add(hostingEnvironment.ApplicationName);
        }

        var descriptors = _shapeDescriptors
            .Where(sd => enabledAndOrderedFeatureIds.Contains(sd.Value.Feature.Id))
            .Where(sd => IsModuleOrRequestedTheme(extensionManager, sd.Value.Feature, themeId))
            .OrderBy(sd => enabledAndOrderedFeatureIds.IndexOf(sd.Value.Feature.Id))
            .GroupBy(sd => sd.Value.ShapeType, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ShapeDescriptorIndex
            (
                shapeType: group.Key,
                alterations: group.Select(kv => kv.Value)
            ))
            .ToList();

        var shapeTable = new ShapeTable
        (
            descriptors: descriptors.ToFrozenDictionary(sd => sd.ShapeType, x => (ShapeDescriptor)x, StringComparer.OrdinalIgnoreCase),
            bindings: descriptors.SelectMany(sd => sd.Bindings).ToFrozenDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase)
        );

        logger.LogInformation("Done building shape table for {Theme}", themeId);

        _shapeTableCache[themeId ?? DefaultThemeIdKey] = Task.FromResult(shapeTable);

        return shapeTable;
    }

    private static void BuildDescriptors(
        IShapeTableProvider bindingStrategy,
        IEnumerable<ShapeAlteration> builtAlterations,
        Dictionary<string, FeatureShapeDescriptor> shapeDescriptors)
    {
        var alterationSets = builtAlterations.GroupBy(a => a.Feature.Id + a.ShapeType);

        foreach (var alterations in alterationSets)
        {
            var firstAlteration = alterations.First();

            var key = bindingStrategy.GetType().Name
                + firstAlteration.Feature.Id
                + firstAlteration.ShapeType.ToLower();

            if (!_shapeDescriptors.ContainsKey(key))
            {
                var descriptor = new FeatureShapeDescriptor
                (
                    firstAlteration.Feature,
                    firstAlteration.ShapeType
                );

                foreach (var alteration in alterations)
                {
                    alteration.Alter(descriptor);
                }

                shapeDescriptors[key] = descriptor;
            }
        }
    }

    private static bool IsModuleOrRequestedTheme(IExtensionManager extensionManager, IFeatureInfo feature, string themeId)
    {
        if (!feature.IsTheme())
        {
            return true;
        }

        if (string.IsNullOrEmpty(themeId))
        {
            return false;
        }

        return feature.Id == themeId || IsBaseTheme(feature.Id, themeId);

        bool IsBaseTheme(string themeFeatureId, string themeId)
        {
            return extensionManager
                .GetFeatureDependencies(themeId)
                .Any(f => f.Id == themeFeatureId);
        }
    }
}
