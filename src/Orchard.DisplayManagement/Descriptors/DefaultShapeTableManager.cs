using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Extensions;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Utility;
using Orchard.Environment.Shell;

namespace Orchard.DisplayManagement.Descriptors
{
    /// <summary>
    /// This class needs to be a singleton per tenant as it can contain different shapes
    /// for each tenant, even if they share the same theme.
    /// </summary>
    public class DefaultShapeTableManager : IShapeTableManager
    {
        private static ConcurrentDictionary<string, FeatureShapeDescriptor> _shapeDescriptors = new ConcurrentDictionary<string, FeatureShapeDescriptor>();

        private readonly IEnumerable<IShapeTableProvider> _bindingStrategies;
        private readonly IShellFeaturesManager _shellFeaturesManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IExtensionOrderingStrategy> _extensionOrderingStrategies;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly ILogger _logger;

        private readonly IMemoryCache _memoryCache;

        public DefaultShapeTableManager(
            IEnumerable<IShapeTableProvider> bindingStrategies,
            IShellFeaturesManager shellFeaturesManager,
            IExtensionManager extensionManager,
            IEnumerable<IExtensionOrderingStrategy> extensionOrderingStrategies,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<DefaultShapeTableManager> logger,
            IMemoryCache memoryCache)
        {
            _bindingStrategies = bindingStrategies;
            _shellFeaturesManager = shellFeaturesManager;
            _extensionManager = extensionManager;
            _extensionOrderingStrategies = extensionOrderingStrategies;
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

                var orderedFeatureIds = _memoryCache.GetOrCreate("OrderedFeatureIds", entry =>
                {
                    entry.Priority = CacheItemPriority.NeverRemove;
                    return _shapeDescriptors.Select(sd => sd.Value.Feature).Distinct()
                        .OrderByDependenciesAndPriorities(HasDependency, f => f.Priority)
                        .Select(f => f.Id).ToList();
                });

                var enabledFeatureIds = _shellFeaturesManager
                    .GetEnabledFeaturesAsync()
                    .GetAwaiter()
                    .GetResult()
                    .Select(f => f.Id)
                    .ToList();

                var descriptors = _shapeDescriptors
                    .Where(sd => IsEnabledModuleOrRequestedTheme(sd.Value, themeId, enabledFeatureIds))
                    .OrderBy(sd => orderedFeatureIds.IndexOf(sd.Value.Feature.Id))
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

        private bool HasDependency(IFeatureInfo f1, IFeatureInfo f2)
        {
            return _extensionOrderingStrategies.Any(s => s.HasDependency(f1, f2)) || f1.Dependencies.Contains(f2.Id);
        }

        private bool IsEnabledModuleOrRequestedTheme(FeatureShapeDescriptor descriptor, string themeName, List<string> enabledFeatureIds)
        {
            return IsEnabledModuleOrRequestedTheme(descriptor.Feature, themeName, enabledFeatureIds);
        }

        private bool IsEnabledModuleOrRequestedTheme(IFeatureInfo feature, string themeName, List<string> enabledFeatureIds)
        {
            return IsModuleOrRequestedTheme(feature, themeName) && (feature.Id == "Core" || enabledFeatureIds.Contains(feature.Id));
        }
         
        private bool IsModuleOrRequestedTheme(IFeatureInfo feature, string themeId)
        {
            if (!feature.Extension.Manifest.IsTheme())
            {
                return true;
            }

            // A null theme means we are looking for any shape in any module or theme
            if (string.IsNullOrEmpty(themeId))
            {
                return true;
            }

            // alterations from themes must be from the given theme or a base theme
            var featureId = feature.Id;
            return string.IsNullOrEmpty(featureId) || featureId == themeId || IsBaseTheme(featureId, themeId);
        }

        private bool IsBaseTheme(string featureId, string themeId)
        {
            // determine if the given feature is a base theme of the given theme
            return _extensionManager
                .GetFeatures(new[] { themeId })
                .Where(x => x.Extension.Manifest.IsTheme())
                .Select(fi => new ThemeExtensionInfo(fi.Extension))
                .Any(x => x.IsBaseThemeFeature(featureId));
        }
    }
}