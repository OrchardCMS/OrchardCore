namespace OrchardCore.Environment.Extensions.Features;

/// <summary>
/// Base class for <see cref="IFeaturesProvider"/> implementations that raises
/// <see cref="IFeatureBuilderEvents.Building"/> and <see cref="IFeatureBuilderEvents.Built"/>
/// around the construction of a feature.
/// </summary>
public abstract class FeaturesProviderBase : IFeaturesProvider
{
    private readonly IEnumerable<IFeatureBuilderEvents> _featureBuilderEvents;

    protected FeaturesProviderBase(IEnumerable<IFeatureBuilderEvents> featureBuilderEvents)
    {
        _featureBuilderEvents = featureBuilderEvents;
    }

    public abstract IEnumerable<IFeatureInfo> GetFeatures(IExtensionInfo extensionInfo, IManifestInfo manifestInfo);

    /// <summary>
    /// Raises <see cref="IFeatureBuilderEvents.Building"/> for the given context, constructs
    /// the feature via <paramref name="createFeatureInfo"/>, then raises
    /// <see cref="IFeatureBuilderEvents.Built"/> for the constructed feature.
    /// </summary>
    protected IFeatureInfo BuildFeature(FeatureBuildingContext context, Func<FeatureBuildingContext, IFeatureInfo> createFeatureInfo)
    {
        foreach (var builder in _featureBuilderEvents)
        {
            builder.Building(context);
        }

        var featureInfo = createFeatureInfo(context);

        foreach (var builder in _featureBuilderEvents)
        {
            builder.Built(featureInfo);
        }

        return featureInfo;
    }
}
