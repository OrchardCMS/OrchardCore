using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell;

namespace Orchard.DisplayManagement.Descriptors
{
    /// <summary>
    /// This class needs to use a cache which is a singleton per tenant as it can contain different shapes
    /// for each tenant, even if they share the same theme.
    /// </summary>
    public class DefaultShapeTableManager : IShapeTableManager
    {
        private static ConcurrentDictionary<string, FeatureShapeDescriptor> _shapeDescriptors = new ConcurrentDictionary<string, FeatureShapeDescriptor>();

        private readonly IEnumerable<IShapeTableProvider> _bindingStrategies;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IExtensionManager _extensionManager;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly ILogger _logger;

        private readonly IMemoryCache _memoryCache;

        public DefaultShapeTableManager(
            IEnumerable<IShapeTableProvider> bindingStrategies,
            IShellFeaturesManager shellFeaturesManager,
            IExtensionManager extensionManager,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<DefaultShapeTableManager> logger,
            IMemoryCache memoryCache)
        {
            _bindingStrategies = bindingStrategies;
            _shellFeaturesManager = shellFeaturesManager;
            _extensionManager = extensionManager;
            _typeFeatureProvider = typeFeatureProvider;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public ShapeTable GetShapeTable(string themeId)
        {
            var cacheKey = $"ShapeTable:{themeId}";

            ShapeTable shapeTable;
            if (!_memoryCache.TryGetValue(cacheKey, out shapeTable))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Start building shape table");
                }

                var excludedFeatures = _shapeDescriptors.Count == 0 ? new List<string>() :
                    _shapeDescriptors.Select(kv => kv.Value.Feature.Id).Distinct().ToList();

                foreach (var bindingStrategy in _bindingStrategies)
                {
                    IFeatureInfo strategyFeature = _typeFeatureProvider.GetFeatureForDependency(bindingStrategy.GetType());

                    if (!(bindingStrategy is IShapeTableHarvester) && excludedFeatures.Contains(strategyFeature.Id))
                        continue;

                    var builder = new ShapeTableBuilder(strategyFeature, excludedFeatures);
                    bindingStrategy.Discover(builder);
                    var builtAlterations = builder.BuildAlterations();

                    BuildDescriptors(bindingStrategy, builtAlterations);
                }

                var enabledAndOrderedFeatureIds = _shellFeaturesManager
                    .GetEnabledFeaturesAsync()
                    .GetAwaiter()
                    .GetResult()
                    .Select(f => f.Id)
                    .ToList();

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
                    ));

                shapeTable = new ShapeTable
                {
                    Descriptors = descriptors.Cast<ShapeDescriptor>().ToDictionary(sd => sd.ShapeType, StringComparer.OrdinalIgnoreCase),
                    Bindings = descriptors.SelectMany(sd => sd.Bindings).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase)
                };

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Done building shape table");
                }

                _memoryCache.Set(cacheKey, shapeTable, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
            }

            return shapeTable;
        }

        private void BuildDescriptors(IShapeTableProvider bindingStrategy, IEnumerable<ShapeAlteration> builtAlterations)
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

                    _shapeDescriptors[key] = descriptor;
                }
            }
        }

        private bool IsModuleOrRequestedTheme(IFeatureInfo feature, string themeId)
        {
            if (!feature.Extension.Manifest.IsTheme())
            {
                return true;
            }

            if (string.IsNullOrEmpty(themeId))
            {
                return true;
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