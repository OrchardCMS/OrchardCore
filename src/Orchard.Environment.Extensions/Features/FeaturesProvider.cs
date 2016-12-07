using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public class FeaturesProvider : IFeatureProvider
    {
        public const string FeatureProviderCacheKey = "FeatureProvider:Features";

        private static string NameKey = "Name";
        private static string PriorityKey = "Priority";
        private static string DependenciesKey = "Dependencies";
        private static string CategoryKey = "Category";
        private static string DescriptionKey = "Description";

        private readonly IEnumerable<IFeatureBuilderEvents> _featureBuilderEvents;

        private readonly ILogger L;

        public FeaturesProvider(
            IEnumerable<IFeatureBuilderEvents> featureBuilderEvents,
            ILogger<FeaturesProvider> logger)
        {
            _featureBuilderEvents = featureBuilderEvents;
            L = logger;
        }

        public IEnumerable<IFeatureInfo> GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo)
        {
            var features = new List<IFeatureInfo>();

            // Features and Dependencies live within this section
            var featuresSectionChildren = manifestInfo.ConfigurationRoot.GetSection("Features").GetChildren().ToList();
            if (featuresSectionChildren.Count > 0)
            {
                foreach (var featureSection in featuresSectionChildren)
                {
                    var featureId = featureSection.Key;

                    var featureDetails = featureSection.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                    var featureName =
                        featureDetails.ContainsKey(NameKey) ?
                            featureDetails[NameKey] : manifestInfo.Name;

                    var featureDependencyIds = featureDetails.ContainsKey(DependenciesKey) ?
                        featureDetails[DependenciesKey]
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .ToArray() : new string[0];

                    var manifestFeatureDetails = manifestInfo
                        .ConfigurationRoot.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                    var featurePriority = featureDetails.ContainsKey(PriorityKey) ?
                            double.Parse(featureDetails[PriorityKey]) :
                            (manifestFeatureDetails.ContainsKey(PriorityKey) ? double.Parse(manifestFeatureDetails[PriorityKey]) : 0D);

                    var featureCategory =
                        featureDetails.ContainsKey(CategoryKey) ?
                            featureDetails[CategoryKey] :
                            (manifestFeatureDetails.ContainsKey(CategoryKey) ? manifestFeatureDetails[CategoryKey] : null);

                    var featureDescription =
                        featureDetails.ContainsKey(DescriptionKey) ?
                            featureDetails[DescriptionKey] :
                            (manifestFeatureDetails.ContainsKey(DescriptionKey) ? manifestFeatureDetails[DescriptionKey] : null);

                    _featureBuilderEvents.Invoke(fbe => fbe.Building(
                        new FeatureBuildingContext
                        {
                            FeatureId = featureId,
                            FeatureName = featureName,
                            Category = featureCategory,
                            Description = featureDescription,
                            ExtensionInfo = extensionInfo,
                            FeatureDetails = featureDetails,
                            ManifestDetails = manifestFeatureDetails,
                            ManifestInfo = manifestInfo,
                            Priority = featurePriority,
                            FeatureDependencyIds = featureDependencyIds
                        }), L);

                    var featureInfo = new FeatureInfo(
                        featureId,
                        featureName,
                        featurePriority,
                        featureCategory,
                        featureDescription,
                        extensionInfo,
                        featureDependencyIds);

                    _featureBuilderEvents.Invoke(fbe => fbe.Built(featureInfo), L);

                    features.Add(featureInfo);
                }
            }
            else
            {
                // The Extension has only one feature, itself, and that can have dependencies
                var featureId = extensionInfo.Id;
                var featureName = manifestInfo.Name;

                var featureDetails = manifestInfo.ConfigurationRoot.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                var featurePriority = featureDetails.ContainsKey(PriorityKey) ? double.Parse(featureDetails[PriorityKey]) : 0D;

                var featureDependencyIds = featureDetails.ContainsKey(DependenciesKey) ?
                    featureDetails[DependenciesKey]
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToArray() : new string[0];

                var featureCategory = featureDetails.ContainsKey(CategoryKey) ? featureDetails[CategoryKey] : null;

                var featureDescription = featureDetails.ContainsKey(DescriptionKey) ? featureDetails[DescriptionKey] : null;

                _featureBuilderEvents.Invoke(fbe => fbe.Building(
                    new FeatureBuildingContext
                    {
                        FeatureId = featureId,
                        FeatureName = featureName,
                        Category = featureCategory,
                        Description = featureDescription,
                        ExtensionInfo = extensionInfo,
                        FeatureDetails = featureDetails,
                        ManifestDetails = featureDetails,
                        ManifestInfo = manifestInfo,
                        Priority = featurePriority
                    }), L);

                var featureInfo = new FeatureInfo(
                    featureId,
                    featureName,
                    featurePriority,
                    featureCategory,
                    featureDescription,
                    extensionInfo,
                    featureDependencyIds);

                _featureBuilderEvents.Invoke(fbe => fbe.Built(featureInfo), L);

                features.Add(featureInfo);
            }

            return features;
        }
    }
}
