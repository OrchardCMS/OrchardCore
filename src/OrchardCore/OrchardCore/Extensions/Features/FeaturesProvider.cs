using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Extensions.Features
{
    /// <inheritdoc/>
    public class FeaturesProvider : IFeaturesProvider
    {
        public const string FeatureProviderStateKey = "FeatureProvider:Features";

        private readonly IEnumerable<IFeatureBuilderEvents> _featureBuilderEvents;

        /// <summary>
        /// Constructs a provider instance.
        /// </summary>
        /// <param name="featureBuilderEvents"></param>
        public FeaturesProvider(IEnumerable<IFeatureBuilderEvents> featureBuilderEvents)
        {
            _featureBuilderEvents = featureBuilderEvents;
        }

        public IEnumerable<IFeatureInfo> GetFeatures(IExtensionInfo extensionInfo, IManifestInfo manifestInfo)
        {
            var featuresInfos = new List<IFeatureInfo>();

            // Features and Dependencies live within this section
            var features = manifestInfo.ModuleInfo.Features.ToList();
            if (features.Count > 0)
            {
                foreach (var feature in features)
                {
                    if (String.IsNullOrWhiteSpace(feature.Id))
                    {
                        throw new ArgumentException(
                            $"A {nameof(feature)} is missing a mandatory '{nameof(feature.Id)}' property in the Module '{extensionInfo.Id}'");
                    }

                    // Attribute properties are transparently resolved by the instances themselves for convenience
                    var featureId = feature.Id;
                    var featureName = feature.Name;

                    var featureDependencyIds = feature.Dependencies;

                    // Categorize, Prioritize, Describe, using the ModuleInfo (ModuleAttribute) as the back stop
                    var featureCategory = feature.Categorize(manifestInfo.ModuleInfo);
                    var featurePriority = feature.Prioritize(manifestInfo.ModuleInfo);
                    var featureDescription = feature.Describe(manifestInfo.ModuleInfo);
                    var featureDefaultTenantOnly = feature.DefaultTenantOnly;
                    var featureIsAlwaysEnabled = feature.IsAlwaysEnabled;
                    var featureEnabledByDependencyOnly = feature.EnabledByDependencyOnly;

                    var context = new FeatureBuildingContext
                    {
                        FeatureId = featureId,
                        FeatureName = featureName,
                        Category = featureCategory,
                        Description = featureDescription,
                        ExtensionInfo = extensionInfo,
                        ManifestInfo = manifestInfo,
                        Priority = featurePriority,
                        FeatureDependencyIds = featureDependencyIds,
                        DefaultTenantOnly = featureDefaultTenantOnly,
                        IsAlwaysEnabled = featureIsAlwaysEnabled,
                        EnabledByDependencyOnly = featureEnabledByDependencyOnly
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
                        context.FeatureDependencyIds,
                        context.DefaultTenantOnly,
                        context.IsAlwaysEnabled,
                        context.EnabledByDependencyOnly);

                    foreach (var builder in _featureBuilderEvents)
                    {
                        builder.Built(featureInfo);
                    }

                    featuresInfos.Add(featureInfo);
                }
            }
            else
            {
                // The Extension has only one feature, itself, and that can have dependencies
                var featureId = extensionInfo.Id;
                var featureName = manifestInfo.Name;

                var featureDependencyIds = manifestInfo.ModuleInfo.Dependencies;

                // Ditto Categorize, Prioritize, Describe, in this case the root Module 'is' the back stop
                var featureCategory = manifestInfo.ModuleInfo.Categorize();
                var featurePriority = manifestInfo.ModuleInfo.Prioritize();
                var featureDescription = manifestInfo.ModuleInfo.Describe();
                var featureDefaultTenantOnly = manifestInfo.ModuleInfo.DefaultTenantOnly;
                var featureIsAlwaysEnabled = manifestInfo.ModuleInfo.IsAlwaysEnabled;
                var featureEnabledByDependencyOnly = manifestInfo.ModuleInfo.EnabledByDependencyOnly;

                var context = new FeatureBuildingContext
                {
                    FeatureId = featureId,
                    FeatureName = featureName,
                    Category = featureCategory,
                    Description = featureDescription,
                    ExtensionInfo = extensionInfo,
                    ManifestInfo = manifestInfo,
                    Priority = featurePriority,
                    FeatureDependencyIds = featureDependencyIds,
                    DefaultTenantOnly = featureDefaultTenantOnly,
                    IsAlwaysEnabled = featureIsAlwaysEnabled,
                    EnabledByDependencyOnly = featureEnabledByDependencyOnly,
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
                    context.FeatureDependencyIds,
                    context.DefaultTenantOnly,
                    context.IsAlwaysEnabled,
                    context.EnabledByDependencyOnly);

                foreach (var builder in _featureBuilderEvents)
                {
                    builder.Built(featureInfo);
                }

                featuresInfos.Add(featureInfo);
            }

            return featuresInfos;
        }
    }
}
