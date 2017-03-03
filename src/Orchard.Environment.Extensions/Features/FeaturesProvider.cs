using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Orchard.Environment.Extensions.Features
{
    public class FeaturesProvider : IFeaturesProvider
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
                            int.Parse(featureDetails[PriorityKey]) :
                            (manifestFeatureDetails.ContainsKey(PriorityKey) ? int.Parse(manifestFeatureDetails[PriorityKey]) : 0);

                    var featureCategory =
                        featureDetails.ContainsKey(CategoryKey) ?
                            featureDetails[CategoryKey] :
                            (manifestFeatureDetails.ContainsKey(CategoryKey) ? manifestFeatureDetails[CategoryKey] : null);

                    var featureDescription =
                        featureDetails.ContainsKey(DescriptionKey) ?
                            featureDetails[DescriptionKey] :
                            (manifestFeatureDetails.ContainsKey(DescriptionKey) ? manifestFeatureDetails[DescriptionKey] : null);

                    var context = new FeatureBuildingContext
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
                    };

                    foreach (var builder in _featureBuilderEvents)
                    {
                        builder.Building(context);
                    }

                    var featureInfo = new FeatureInfo(
                        featureId,
                        featureName,
                        featurePriority,
                        featureCategory,
                        featureDescription,
                        extensionInfo,
                        featureDependencyIds);

                    foreach (var builder in _featureBuilderEvents)
                    {
                        builder.Built(featureInfo);
                    }
                    
                    features.Add(featureInfo);
                }
            }
            else
            {
                // The Extension has only one feature, itself, and that can have dependencies
                var featureId = extensionInfo.Id;
                var featureName = manifestInfo.Name;

                var featureDetails = manifestInfo.ConfigurationRoot.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                var featurePriority = featureDetails.ContainsKey(PriorityKey) ? int.Parse(featureDetails[PriorityKey]) : 0;

                var featureDependencyIds = featureDetails.ContainsKey(DependenciesKey) ?
                    featureDetails[DependenciesKey]
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToArray() : new string[0];

                var featureCategory = featureDetails.ContainsKey(CategoryKey) ? featureDetails[CategoryKey] : null;

                var featureDescription = featureDetails.ContainsKey(DescriptionKey) ? featureDetails[DescriptionKey] : null;

                var context = new FeatureBuildingContext
                {
                    FeatureId = featureId,
                    FeatureName = featureName,
                    Category = featureCategory,
                    Description = featureDescription,
                    ExtensionInfo = extensionInfo,
                    FeatureDetails = featureDetails,
                    ManifestDetails = featureDetails,
                    ManifestInfo = manifestInfo,
                    Priority = featurePriority,
                    FeatureDependencyIds = featureDependencyIds
                };

                foreach (var builder in _featureBuilderEvents)
                {
                    builder.Building(context);
                }

                var featureInfo = new FeatureInfo(
                    context.FeatureId,
                    context.FeatureName,
                    context.Priority,
                    context.Category,
                    context.Description,
                    context.ExtensionInfo,
                    context.FeatureDependencyIds);

                foreach (var builder in _featureBuilderEvents)
                {
                    builder.Built(featureInfo);
                }

                features.Add(featureInfo);
            }

            return features;
        }
    }
}
