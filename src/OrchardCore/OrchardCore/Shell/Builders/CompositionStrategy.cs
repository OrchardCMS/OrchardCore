using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using System.Collections.Specialized;

namespace OrchardCore.Environment.Shell.Builders
{
    public class CompositionStrategy : ICompositionStrategy
    {
        private readonly IExtensionManager _extensionManager;
        private readonly ILogger _logger;

        public CompositionStrategy(
            IExtensionManager extensionManager,
            ILogger<CompositionStrategy> logger)
        {
            _extensionManager = extensionManager;
            _logger = logger;
        }

        public async Task<ShellBlueprint> ComposeAsync(ShellSettings settings, ShellDescriptor descriptor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Composing blueprint");
            }

            var featureIds = descriptor.Features.Select(x => x.Id).ToArray();
            var features = await _extensionManager.LoadFeaturesAsync(featureIds);

            var typesFeatures = new List<TypeFeatureEntry>();
            var typesRequiredFeatures = new List<TypeFeatureEntry>();
            var typesFeaturesMaxOrders = new Dictionary<string, int>();

            // Enlist all type feature entries in the current order of enabled features.
            var order = 0;
            foreach (var feature in features)
            {
                foreach (var type in feature.ExportedTypes)
                {
                    var requiredFeatures = RequireFeaturesAttribute.GetRequiredFeatureNamesForType(type);
                    if (requiredFeatures.All(id => featureIds.Contains(id)))
                    {
                        if (requiredFeatures.Count > 0)
                        {
                            // Add a type feature entry having at least a required feature.
                            typesRequiredFeatures.Add(new TypeFeatureEntry(type, feature, order, requiredFeatures));
                        }
                        else
                        {
                            // Update the max order of entries for this feature.
                            typesFeaturesMaxOrders[feature.FeatureInfo.Id] = order;
                            typesFeatures.Add(new TypeFeatureEntry(type, feature, order));
                        }

                        // Keep odd values that will be used for entries having required features.
                        order += 2;
                    }
                }
            }

            // Adjust order of entries having required features.
            foreach (var typeFeature in typesRequiredFeatures)
            {
                foreach (var requiredFeature in typeFeature.RequiredFeatures)
                {
                    if (typesFeaturesMaxOrders.TryGetValue(requiredFeature, out var maxOrder) &&
                        maxOrder > typeFeature.Order)
                    {
                        // Use odd values to move down the entry.
                        typeFeature.Order = maxOrder + 1;
                    }
                }
            }

            var dependencies = typesFeatures
                .Concat(typesRequiredFeatures)
                .OrderBy(e => e.Order)
                .ToDictionary(e => e.Type, e => e.Feature);

            var result = new ShellBlueprint
            {
                Settings = settings,
                Descriptor = descriptor,
                Dependencies = dependencies
            };

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Done composing blueprint");
            }

            return result;
        }

        internal class TypeFeatureEntry
        {
            public TypeFeatureEntry(Type type, FeatureEntry feature, int order, IList<string> requiredFeatures = null)
            {
                Type = type;
                Feature = feature;
                RequiredFeatures = requiredFeatures;
                Order = order;
            }

            public Type Type { get; }
            public FeatureEntry Feature { get; }
            public IList<string> RequiredFeatures { get; }
            public int Order { get; set; }
        }
    }
}
