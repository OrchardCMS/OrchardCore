using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Extensions.Utility;
using Orchard.Events;
using Orchard.Utility;

namespace Orchard.DisplayManagement.Descriptors
{
    /// <summary>
    /// This class needs to be a singleton per tenant as it can contain different shapes
    /// for each tenant, even if they share the same theme.
    /// </summary>
    public class DefaultShapeTableManager : IShapeTableManager
    {
        private static ConcurrentDictionary<int, FeatureShapeDescriptor> _shapeDescriptors = new ConcurrentDictionary<int, FeatureShapeDescriptor>();

        private readonly IEnumerable<IShapeTableProvider> _bindingStrategies;
        private readonly IExtensionManager _extensionManager;
        private readonly IFeatureManager _featureManager;
        private readonly IEventBus _eventBus;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly ILogger _logger;

        private readonly IMemoryCache _memoryCache;

        public DefaultShapeTableManager(
            IEnumerable<IShapeTableProvider> bindingStrategies,
            IExtensionManager extensionManager,
            IFeatureManager featureManager,
            IEventBus eventBus,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<DefaultShapeTableManager> logger,
            IMemoryCache memoryCache)
        {
            _bindingStrategies = bindingStrategies;
            _extensionManager = extensionManager;
            _featureManager = featureManager;
            _eventBus = eventBus;
            _typeFeatureProvider = typeFeatureProvider;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public ShapeTable GetShapeTable(string themeName)
        {
            var cacheKey = $"ShapeTable:{themeName}";

            ShapeTable shapeTable;
            if (!_memoryCache.TryGetValue(cacheKey, out shapeTable))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Start building shape table");
                }

                var excludedFeatures = _shapeDescriptors.Count == 0 ? new List<string>() :
                    _shapeDescriptors.Select(kv => kv.Value.Feature.Descriptor.Id).Distinct().ToList();

                foreach (var bindingStrategy in _bindingStrategies)
                {
                    Feature strategyDefaultFeature = _typeFeatureProvider.GetFeatureForDependency(bindingStrategy.GetType());

                    if (!(bindingStrategy is IShapeTableHarvester) && excludedFeatures.Contains(strategyDefaultFeature.Descriptor.Id))
                        continue;

                    var builder = new ShapeTableBuilder(strategyDefaultFeature, excludedFeatures);
                    bindingStrategy.Discover(builder);
                    var builtAlterations = builder.BuildAlterations().ToReadOnlyCollection();

                    foreach (var alteration in builtAlterations)
                    {
                        var key = (bindingStrategy.GetType().Name + ":"
                            + alteration.Feature.Descriptor.Id + ":"
                            + alteration.ShapeType).ToLower().GetHashCode();

                        var descriptor = new FeatureShapeDescriptor(alteration.Feature, alteration.ShapeType);

                        alteration.Alter(descriptor);
                        _shapeDescriptors[key] = descriptor;
                    }
                }

                var enabledFeatureIds = _featureManager.GetEnabledFeaturesAsync().Result.Select(fd => fd.Extension.Id).ToList();

                var descriptors = _shapeDescriptors
                    .Where(sd => IsEnabledModuleOrRequestedTheme(sd.Value, themeName, enabledFeatureIds))
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

                //await _eventBus.NotifyAsync<IShapeTableEventHandler>(x => x.ShapeTableCreated(result));

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Done building shape table");
                }

                _memoryCache.Set(cacheKey, shapeTable, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
            }

            return shapeTable;
        }

        private static int GetPriority(KeyValuePair<int, FeatureShapeDescriptor> shapeDescriptor)
        {
            return shapeDescriptor.Value.Feature.Descriptor.Priority;
        }

        private bool DescriptorHasDependency(KeyValuePair<int, FeatureShapeDescriptor> item, KeyValuePair<int, FeatureShapeDescriptor> subject)
        {
            return _extensionManager.HasDependency(item.Value.Feature.Descriptor, subject.Value.Feature.Descriptor);
        }

        private bool IsEnabledModuleOrRequestedTheme(FeatureShapeDescriptor descriptor, string themeName, List<string> enabledFeatureIds)
        {
            return IsEnabledModuleOrRequestedTheme(descriptor?.Feature?.Descriptor, themeName, enabledFeatureIds);
        }

        private bool IsEnabledModuleOrRequestedTheme(FeatureDescriptor descriptor, string themeName, List<string> enabledFeatureIds)
        {
            var id = descriptor?.Id;
            var extensionType = descriptor?.Extension?.ExtensionType;

            return IsModuleOrRequestedTheme(descriptor, themeName) && (DefaultExtensionTypes.IsCore(extensionType)
                || enabledFeatureIds.Contains(id));
        }

        private bool IsModuleOrRequestedTheme(FeatureDescriptor descriptor, string themeName)
        {
            if (descriptor?.Extension == null)
            {
                return false;
            }

            var extensionType = descriptor.Extension.ExtensionType;

            if(DefaultExtensionTypes.IsCore(extensionType))
            {
                // O2: The alteration must be coming from a library, e.g. Orchard.DisplayManagement
                return true;
            }

            if (DefaultExtensionTypes.IsModule(extensionType))
            {
                return true;
            }

            if (DefaultExtensionTypes.IsTheme(extensionType))
            {
                // A null theme means we are looking for any shape in any module or theme
                if (String.IsNullOrEmpty(themeName))
                {
                    return true;
                }

                // alterations from themes must be from the given theme or a base theme
                var featureName = descriptor.Id;
                return string.IsNullOrEmpty(featureName) || featureName == themeName || IsBaseTheme(featureName, themeName);
            }

            return false;
        }

        private bool IsBaseTheme(string featureName, string themeName)
        {
            // determine if the given feature is a base theme of the given theme
            var availableFeatures = _extensionManager.AvailableFeatures();

            var themeFeature = availableFeatures.SingleOrDefault(fd => fd.Id == themeName);
            while (themeFeature != null)
            {
                var baseTheme = themeFeature.Extension.BaseTheme;
                if (string.IsNullOrEmpty(baseTheme))
                {
                    return false;
                }
                if (featureName == baseTheme)
                {
                    return true;
                }
                themeFeature = availableFeatures.SingleOrDefault(fd => fd.Id == baseTheme);
            }
            return false;
        }
    }
}