using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public class FeatureManager : IFeatureManager
    {
        public const string FeatureManagerCacheKey = "FeatureManager:Features";

        private static string NameKey = "Name";
        private static string PriorityKey = "Priority";
        private static string DependenciesKey = "Dependencies";
        private static string CategoryKey = "Category";
        private static string DescriptionKey = "Description";

        public IFeatureInfoList GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo)
        {
            var features = new List<IFeatureInfo>();

            // Features and Dependencies live within this section
            var featuresSection = manifestInfo.ConfigurationRoot.GetSection("Features");
            if (featuresSection.Value != null)
            {
                foreach (var featureSection in featuresSection.GetChildren())
                {
                    var featureId = featureSection.Key;

                    var featureDetails = featureSection.GetChildren().ToDictionary(x => x.Key, v => v.Value);
                    
                    var featureName =
                        featureDetails.ContainsKey(NameKey) ?
                            featureDetails[NameKey] : manifestInfo.Name;

                    // TODO (ngm) look at priority
                    var featurePriority = featureDetails.ContainsKey(PriorityKey) ?
                            double.Parse(featureDetails[PriorityKey]) : 0D;

                    var featureDependencyIds = featureDetails.ContainsKey(DependenciesKey) ?
                        featureDetails[DependenciesKey]
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim())
                            .ToArray() : new string[0];

                    var manifestFeatureDetails = manifestInfo
                        .ConfigurationRoot.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                    var featureCategory = 
                        featureDetails.ContainsKey(CategoryKey) ? 
                            featureDetails[CategoryKey] :
                            (manifestFeatureDetails.ContainsKey(CategoryKey) ? manifestFeatureDetails[CategoryKey] : null);

                    var featureDescription =
                        featureDetails.ContainsKey(DescriptionKey) ?
                            featureDetails[DescriptionKey] :
                            (manifestFeatureDetails.ContainsKey(DescriptionKey) ? manifestFeatureDetails[DescriptionKey] : null);

                    var featureInfo = new FeatureInfo(
                        featureId,
                        featureName,
                        featurePriority,
                        featureCategory,
                        featureDescription,
                        extensionInfo,
                        featureDependencyIds);

                    features.Add(featureInfo);
                }
            }
            else
            {
                // The Extension has only one feature, itself, and that can have dependencies
                var featureId = extensionInfo.ExtensionFileInfo.Name;
                var featureName = manifestInfo.Name;

                var featureDetails = manifestInfo.ConfigurationRoot.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                // TODO (ngm) look at priority
                var featurePriority = 0D;

                var featureDependencyIds = featureDetails.ContainsKey(DependenciesKey) ?
                    featureDetails[DependenciesKey]
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToArray() : new string[0];

                var featureCategory = featureDetails.ContainsKey(CategoryKey) ? featureDetails[CategoryKey] : null;

                var featureDescription = featureDetails.ContainsKey(DescriptionKey) ? featureDetails[DescriptionKey] : null;

                var featureInfo = new FeatureInfo(
                    featureId,
                    featureName,
                    featurePriority,
                    featureCategory,
                    featureDescription,
                    extensionInfo,
                    featureDependencyIds);

                features.Add(featureInfo);
            }

            return new FeatureInfoList(features);
        }
    }
}
