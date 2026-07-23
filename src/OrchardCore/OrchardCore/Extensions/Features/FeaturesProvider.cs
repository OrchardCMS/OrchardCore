namespace OrchardCore.Environment.Extensions.Features;

/// <inheritdoc/>
public sealed class FeaturesProvider : FeaturesProviderBase
{
    public const string FeatureProviderStateKey = "FeatureProvider:Features";

    /// <summary>
    /// Constructs a provider instance.
    /// </summary>
    /// <param name="featureBuilderEvents"></param>
    public FeaturesProvider(IEnumerable<IFeatureBuilderEvents> featureBuilderEvents)
        : base(featureBuilderEvents)
    {
    }

    public override IEnumerable<IFeatureInfo> GetFeatures(IExtensionInfo extensionInfo, IManifestInfo manifestInfo)
    {
        var featuresInfos = new List<IFeatureInfo>();

        // Features and Dependencies live within this section
        var features = manifestInfo.ModuleInfo.Features;
        if (features.Count > 0)
        {
            foreach (var feature in features)
            {
                if (string.IsNullOrWhiteSpace(feature.Id))
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
                    EnabledByDependencyOnly = featureEnabledByDependencyOnly,
                };

                var featureInfo = BuildFeature(context, ctx => new FeatureInfo(
                    ctx.FeatureId,
                    ctx.FeatureName,
                    ctx.Priority,
                    ctx.Category,
                    ctx.Description,
                    ctx.ExtensionInfo,
                    ctx.FeatureDependencyIds,
                    ctx.DefaultTenantOnly,
                    ctx.IsAlwaysEnabled,
                    ctx.EnabledByDependencyOnly));

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

            var featureInfo = BuildFeature(context, ctx => new FeatureInfo(
                ctx.FeatureId,
                ctx.FeatureName,
                ctx.Priority,
                ctx.Category,
                ctx.Description,
                ctx.ExtensionInfo,
                ctx.FeatureDependencyIds,
                ctx.DefaultTenantOnly,
                ctx.IsAlwaysEnabled,
                ctx.EnabledByDependencyOnly));

            featuresInfos.Add(featureInfo);
        }

        return featuresInfos;
    }
}
