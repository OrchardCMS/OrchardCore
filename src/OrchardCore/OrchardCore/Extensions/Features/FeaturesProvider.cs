namespace OrchardCore.Environment.Extensions.Features;

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

        var features = manifestInfo.ModuleInfo.Features;

        // Synthesize the main feature from module-level metadata in two cases:
        //   1. No explicit [Feature] attributes at all (original single-feature-extension path).
        //   2. The extension is a theme that has explicit sub-features but has NOT declared its
        //      own main feature — without synthesis IsTheme() can never return true for the
        //      extension, and the Themes admin page cannot find it.
        // Regular modules whose explicit features intentionally carry different IDs (e.g. Sample1,
        // Sample2, …) satisfy neither condition and are left unchanged.
        if (features.Count == 0 ||
            (manifestInfo.Type.Equals("Theme", StringComparison.OrdinalIgnoreCase) &&
             !features.Any(f => f.Id == extensionInfo.Id)))
        {
            var mainContext = new FeatureBuildingContext
            {
                FeatureId = extensionInfo.Id,
                FeatureName = manifestInfo.Name,
                Category = manifestInfo.ModuleInfo.Categorize(),
                Description = manifestInfo.ModuleInfo.Describe(),
                ExtensionInfo = extensionInfo,
                ManifestInfo = manifestInfo,
                Priority = manifestInfo.ModuleInfo.Prioritize(),
                FeatureDependencyIds = manifestInfo.ModuleInfo.Dependencies,
                DefaultTenantOnly = manifestInfo.ModuleInfo.DefaultTenantOnly,
                IsAlwaysEnabled = manifestInfo.ModuleInfo.IsAlwaysEnabled,
                EnabledByDependencyOnly = manifestInfo.ModuleInfo.EnabledByDependencyOnly,
            };

            foreach (var builder in _featureBuilderEvents)
            {
                builder.Building(mainContext);
            }

            var mainFeatureInfo = new FeatureInfo(
                mainContext.FeatureId,
                mainContext.FeatureName,
                mainContext.Priority,
                mainContext.Category,
                mainContext.Description,
                mainContext.ExtensionInfo,
                mainContext.FeatureDependencyIds,
                mainContext.DefaultTenantOnly,
                mainContext.IsAlwaysEnabled,
                mainContext.EnabledByDependencyOnly);

            foreach (var builder in _featureBuilderEvents)
            {
                builder.Built(mainFeatureInfo);
            }

            featuresInfos.Add(mainFeatureInfo);
        }

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
