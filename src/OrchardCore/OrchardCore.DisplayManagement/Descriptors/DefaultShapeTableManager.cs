using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

namespace OrchardCore.DisplayManagement.Descriptors
{
    /// <summary>
    /// This class needs to use a cache which is a singleton per tenant as it can contain different shapes
    /// for each tenant, even if they share the same theme.
    /// </summary>
    public class DefaultShapeTableManager : IShapeTableManager
    {
        private static readonly ConcurrentDictionary<string, FeatureShapeDescriptor> _shapeDescriptors = new();
        private static readonly object _syncLock = new();

        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IEnumerable<IShapeTableProvider> _bindingStrategies;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IExtensionManager _extensionManager;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public DefaultShapeTableManager(
            IHostEnvironment hostingEnvironment,
            IEnumerable<IShapeTableProvider> bindingStrategies,
            IShellFeaturesManager shellFeaturesManager,
            IExtensionManager extensionManager,
            ITypeFeatureProvider typeFeatureProvider,
            IMemoryCache memoryCache,
            ILogger<DefaultShapeTableManager> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _bindingStrategies = bindingStrategies;
            _shellFeaturesManager = shellFeaturesManager;
            _extensionManager = extensionManager;
            _typeFeatureProvider = typeFeatureProvider;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public ShapeTable GetShapeTable(string themeId)
        {
            var cacheKey = $"ShapeTable:{themeId}";

            if (!_memoryCache.TryGetValue(cacheKey, out ShapeTable shapeTable))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Start building shape table");
                }

                HashSet<string> excludedFeatures;

                // Here we don't use a lock for thread safety but for atomicity.
                lock (_syncLock)
                {
                    excludedFeatures = new HashSet<string>(_shapeDescriptors.Select(kv => kv.Value.Feature.Id));
                }

                var shapeDescriptors = new Dictionary<string, FeatureShapeDescriptor>();

                foreach (var bindingStrategy in _bindingStrategies)
                {
                    var strategyFeature = _typeFeatureProvider.GetFeatureForDependency(bindingStrategy.GetType());

                    var builder = new ShapeTableBuilder(strategyFeature, excludedFeatures);
                    bindingStrategy.Discover(builder);
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

                var enabledAndOrderedFeatureIds = _shellFeaturesManager
                    .GetEnabledFeaturesAsync()
                    .GetAwaiter()
                    .GetResult()
                    .Select(f => f.Id)
                    .ToList();

                // let the application acting as a super theme for shapes rendering.
                if (enabledAndOrderedFeatureIds.Remove(_hostingEnvironment.ApplicationName))
                {
                    enabledAndOrderedFeatureIds.Add(_hostingEnvironment.ApplicationName);
                }

                var descriptors = _shapeDescriptors
                    .Where(sd => enabledAndOrderedFeatureIds.Contains(sd.Value.Feature.Id))
                    .Where(sd => IsModuleOrRequestedTheme(sd.Value.Feature, themeId))
                    .OrderBy(sd => enabledAndOrderedFeatureIds.IndexOf(sd.Value.Feature.Id))
                    .GroupBy(sd => sd.Value.ShapeType, StringComparer.OrdinalIgnoreCase)
                    .Select(group => new ShapeDescriptorIndex
                    (
                        shapeType: group.Key,
                        alterationKeys: group.Select(kv => kv.Key),
                        descriptors: _shapeDescriptors
                    ))
                    .ToList();

                shapeTable = new ShapeTable
                (
                    descriptors: descriptors.ToDictionary(sd => sd.ShapeType, x => (ShapeDescriptor)x, StringComparer.OrdinalIgnoreCase),
                    bindings: descriptors.SelectMany(sd => sd.Bindings).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase)
                );

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Done building shape table");
                }

                _memoryCache.Set(cacheKey, shapeTable, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
            }

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

        private bool IsModuleOrRequestedTheme(IFeatureInfo feature, string themeId)
        {
            if (!feature.IsTheme())
            {
                return true;
            }

            if (String.IsNullOrEmpty(themeId))
            {
                return false;
            }

            return feature.Id == themeId || IsBaseTheme(feature.Id, themeId);
        }

        private bool IsBaseTheme(string themeFeatureId, string themeId)
        {
            return _extensionManager
                .GetFeatureDependencies(themeId)
                .Any(f => f.Id == themeFeatureId);
        }
    }
}
