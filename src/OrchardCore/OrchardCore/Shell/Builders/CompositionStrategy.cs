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

            var featureNames = descriptor.Features.Select(x => x.Id).ToArray();
            var features = await _extensionManager.LoadFeaturesAsync(featureNames);

            var dependenciesEntries = new OrderedDictionary();
            var requireFeaturesEntries = new List<RequireFeaturesEntry>();
            var lastIndexesByFeatureId = new Dictionary<string, int>();

            for (var i = 0; i < features.Count(); i++)
            {
                var feature = features.ElementAt(i);
                for (var n = 0; n < feature.ExportedTypes.Count(); n++)
                {
                    var exportedType = feature.ExportedTypes.ElementAt(n);

                    var requiredFeatures = RequireFeaturesAttribute.GetRequiredFeatureNamesForType(exportedType);
                    if (requiredFeatures.All(id => featureNames.Contains(id)))
                    {
                        if (requiredFeatures.Count > 0)
                        {
                            requireFeaturesEntries.Add(new RequireFeaturesEntry(exportedType, feature, requiredFeatures, i + n));
                        }

                        lastIndexesByFeatureId[feature.FeatureInfo.Id] = i + n;
                        dependenciesEntries.Add(exportedType, feature);
                    }
                }
            }

            // Move down the types according to their required features.
            foreach (var require in requireFeaturesEntries)
            {
                var requireIndex = require.Index;
                foreach (var requiredFeature in require.RequiredFeatures)
                {
                    if (lastIndexesByFeatureId.TryGetValue(required, out var index) && index > requireIndex)
                    {
                        requireIndex = index;
                    }
                }

                if (requireIndex != require.Index)
                {
                    dependenciesEntries.RemoveAt(require.Index);
                    dependenciesEntries.Insert(requireIndex, require.Type, require.Feature);
                }
            }

            var dependencies = dependenciesEntries.Cast<DictionaryEntry>()
                .ToDictionary(e => (Type)e.Key, e => (FeatureEntry)e.Value);

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

        internal class RequireFeaturesEntry
        {
            public RequireFeaturesEntry(Type type, FeatureEntry feature, IList<string> requiredFeatures, int index)
            {
                Type = type;
                Feature = feature;
                RequiredFeatures = requiredFeatures;
                Index = index;
            }

            public Type Type { get; }
            public FeatureEntry Feature { get; }
            public IList<string> RequiredFeatures { get; }
            public int Index { get; }
        }
    }
}
