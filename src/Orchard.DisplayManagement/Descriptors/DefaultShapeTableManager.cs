using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Extensions;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Utility;
using Orchard.Environment.Shell;
using System.Threading.Tasks;

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

                var enabledFeatureIds = _shellFeaturesManager.GetEnabledFeaturesAsync()
                    .GetAwaiter().GetResult().Select(f => f.Id).ToList();

                var descriptors = _shapeDescriptors
                    .Where(sd => IsEnabledModuleOrRequestedTheme(sd.Value, themeId, enabledFeatureIds))
                    .OrderByDependenciesAndPriorities(DescriptorHasDependency, GetPriority)
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

        private static double GetPriority(KeyValuePair<string, FeatureShapeDescriptor> shapeDescriptor)
        {
            return shapeDescriptor.Value.Feature.Priority;
        }

        private bool DescriptorHasDependency(KeyValuePair<string, FeatureShapeDescriptor> item, KeyValuePair<string, FeatureShapeDescriptor> subject)
        {
            if (item.Value.Feature.Extension.Manifest.IsTheme())
            {
                if (subject.Value.Feature.Id == "Core")
                {
                    return true;
                }

                if (subject.Value.Feature.Extension.Manifest.IsModule())
                {
                    return true;
                }

                if (subject.Value.Feature.Extension.Manifest.IsTheme())
                {
                    var theme = new ThemeExtensionInfo(item.Value.Feature.Extension);

                    if (theme.HasBaseTheme())
                    {
                        return theme.BaseTheme == subject.Value.Feature.Id;
                    }
                }
            }

            return item.Value.Feature.Dependencies?.Contains(subject.Value.Feature.Id) ?? false;
        }

        private bool IsEnabledModuleOrRequestedTheme(FeatureShapeDescriptor descriptor, string themeName, List<string> enabledFeatureIds)
        {
            return IsEnabledModuleOrRequestedTheme(descriptor?.Feature, themeName, enabledFeatureIds);
        }

        private bool IsEnabledModuleOrRequestedTheme(IFeatureInfo feature, string themeName, List<string> enabledFeatureIds)
        {
            return IsModuleOrRequestedTheme(feature, themeName) && (feature?.Id == "Core" || enabledFeatureIds.Contains(feature?.Id));
        }

        private bool IsModuleOrRequestedTheme(IFeatureInfo feature, string themeId)
        {
            if (feature?.Extension == null)
            {
                return false;
            }

            if (feature.Id == "Core")
            {
                // O2: The feature must be coming from a core library, e.g. Orchard.DisplayManagement
                return true;
            }

            if (feature.Extension.Manifest.IsModule())
            {
                return true;
            }

            if (feature.Extension.Manifest.IsTheme())
            {
                // A null theme means we are looking for any shape in any module or theme
                if (String.IsNullOrEmpty(themeId))
                {
                    return true;
                }

                // alterations from themes must be from the given theme or a base theme
                var featureId = feature.Id;
                return string.IsNullOrEmpty(featureId) || featureId == themeId || IsBaseTheme(featureId, themeId);
            }

            return false;
        }

        private bool IsBaseTheme(string featureId, string themeId)
        {
            // determine if the given feature is a base theme of the given theme
            var availableFeatures = _extensionManager.GetFeatures();

            var themeFeature = availableFeatures.SingleOrDefault(f => f.Id == themeId);
            while (themeFeature != null && themeFeature.Extension.Manifest.IsTheme())
            {
                var themeExtensionInfo = new ThemeExtensionInfo(themeFeature.Extension);
                if (!themeExtensionInfo.HasBaseTheme())
                {
                    return false;
                }
                if (themeExtensionInfo.IsBaseThemeFeature(featureId))
                {
                    return true;
                }
                themeFeature = availableFeatures.SingleOrDefault(f => f.Id == themeExtensionInfo.BaseTheme);
            }
            return false;
        }
    }
}