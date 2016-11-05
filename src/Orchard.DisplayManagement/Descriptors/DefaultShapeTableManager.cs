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
        private static ConcurrentDictionary<string, ShapeAlteration> shapeAlterations = new ConcurrentDictionary<string, ShapeAlteration>(StringComparer.OrdinalIgnoreCase);

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

                var excludedFeatures = shapeAlterations
                    .GroupBy(kvp => kvp.Value.Feature.Descriptor).Select(g => g.First().Value.Feature.Descriptor)
                    .Where(shapeAlteration => IsNonCoreEnabledModuleOrRequestedTheme(shapeAlteration, themeName))
                    .ToList();

                IList<IReadOnlyList<ShapeAlteration>> alterationSets = new List<IReadOnlyList<ShapeAlteration>>();
                foreach (var bindingStrategy in _bindingStrategies)
                {
                    Feature strategyDefaultFeature =
                        _typeFeatureProvider.GetFeatureForDependency(bindingStrategy.GetType());

                    var builder = new ShapeTableBuilder(strategyDefaultFeature, excludedFeatures);

                    bindingStrategy.Discover(builder);

                    var builtAlterations = builder.BuildAlterations().ToReadOnlyCollection();
                    if (builtAlterations.Any())
                    {
                        alterationSets.Add(builtAlterations);
                    }

                    foreach (var alteration in builtAlterations)
                    {
                        var key = bindingStrategy.GetType().Name + ":"
                            + alteration.Feature.Descriptor.Id + ":"
                            + alteration.ShapeType;

                        if (!shapeAlterations.ContainsKey(key))
                        {
                            shapeAlterations[key] = alteration;
                        }
                    }
                }

                var alterations = shapeAlterations
                .Select(shapeAlteration => shapeAlteration.Value)
                .Where(alteration => IsEnabledModuleOrRequestedTheme(alteration, themeName))
                .OrderByDependenciesAndPriorities(AlterationHasDependency, GetPriority)
                .ToList();

                var descriptors = alterations.GroupBy(alteration => alteration.ShapeType, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.Aggregate(
                        new ShapeDescriptor { ShapeType = group.Key },
                        (descriptor, alteration) =>
                        {
                            alteration.Alter(descriptor);
                            return descriptor;
                        })).ToList();

                foreach (var descriptor in descriptors)
                {
                    foreach (var alteration in alterations.Where(a => a.ShapeType == descriptor.ShapeType).ToList())
                    {
                        var local = new ShapeDescriptor { ShapeType = descriptor.ShapeType };
                        alteration.Alter(local);
                        descriptor.BindingSources.Add(local.BindingSource);
                    }
                }

                shapeTable = new ShapeTable
                {
                    Descriptors = descriptors.ToDictionary(sd => sd.ShapeType, StringComparer.OrdinalIgnoreCase),
                    Bindings = descriptors.SelectMany(sd => sd.Bindings).ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase),
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

        private static int GetPriority(ShapeAlteration shapeAlteration)
        {
            return shapeAlteration.Feature.Descriptor.Priority;
        }

        private bool AlterationHasDependency(ShapeAlteration item, ShapeAlteration subject)
        {
            return _extensionManager.HasDependency(item.Feature.Descriptor, subject.Feature.Descriptor);
        }

        private bool IsEnabledModuleOrRequestedTheme(ShapeAlteration alteration, string themeName)
        {
            return IsEnabledModuleOrRequestedTheme(alteration?.Feature?.Descriptor, themeName);
        }

        private bool IsEnabledModuleOrRequestedTheme(FeatureDescriptor descriptor, string themeName)
        {
            var id = descriptor?.Id;
            var extensionType = descriptor?.Extension?.ExtensionType;

            return IsModuleOrRequestedTheme(descriptor, themeName) && (String.IsNullOrEmpty(extensionType)
                || _featureManager.GetEnabledFeaturesAsync().Result.FirstOrDefault(x => x.Id == id) != null);
        }

        private bool IsNonCoreEnabledModuleOrRequestedTheme(FeatureDescriptor descriptor, string themeName)
        {
            var id = descriptor?.Id;

            return IsModuleOrRequestedTheme(descriptor, themeName)
                && _featureManager.GetEnabledFeaturesAsync().Result.FirstOrDefault(x => x.Id == id) != null;
        }

        private bool IsModuleOrRequestedTheme(FeatureDescriptor descriptor, string themeName)
        {
            if (descriptor?.Extension == null)
            {
                return false;
            }

            var extensionType = descriptor.Extension.ExtensionType;

            if(String.IsNullOrEmpty(extensionType))
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