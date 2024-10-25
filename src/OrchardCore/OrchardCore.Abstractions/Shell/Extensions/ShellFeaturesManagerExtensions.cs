using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell;

public static class ShellFeaturesManagerExtensions
{
    public static Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IFeatureInfo> features)
    {
        return shellFeaturesManager.EnableFeaturesAsync(features, false);
    }

    public static async Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IFeatureInfo> features, bool force)
    {
        var (_, featuresToEnable) = await shellFeaturesManager.UpdateFeaturesAsync([], features, force);

        return featuresToEnable;
    }

    public static async Task EnableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager, params string[] featureIds)
    {
        ArgumentNullException.ThrowIfNull(featureIds);

        if (featureIds.Length == 0)
        {
            return;
        }

        var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();

        var featuresToEnable = availableFeatures.Where(feature => featureIds.Contains(feature.Id));

        await shellFeaturesManager.EnableFeaturesAsync(featuresToEnable, force: false);
    }

    public static async Task UpdateFeaturesAsync(this IShellFeaturesManager shellFeaturesManager, IEnumerable<string> featureIdsToDisable, IEnumerable<string> featureIdsToEnable)
    {
        ArgumentNullException.ThrowIfNull(featureIdsToEnable);
        ArgumentNullException.ThrowIfNull(featureIdsToDisable);

        var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();

        var featuresToDisable = availableFeatures.Where(feature => featureIdsToDisable.Contains(feature.Id));
        var featuresToEnable = availableFeatures.Where(feature => featureIdsToEnable.Contains(feature.Id));

        await shellFeaturesManager.UpdateFeaturesAsync(featuresToDisable, featuresToEnable, force: false);
    }

    public static async Task DisableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager, params string[] featureIds)
    {
        ArgumentNullException.ThrowIfNull(featureIds);

        if (featureIds.Length == 0)
        {
            return;
        }

        var availableFeatures = await shellFeaturesManager.GetAvailableFeaturesAsync();

        var featuresToEnable = availableFeatures.Where(feature => featureIds.Contains(feature.Id));

        await shellFeaturesManager.DisableFeaturesAsync(featuresToEnable, force: false);
    }

    public static Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IFeatureInfo> features)
    {
        return shellFeaturesManager.DisableFeaturesAsync(features, false);
    }

    public static async Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IFeatureInfo> features, bool force)
    {
        var (featuresToDisable, _) = await shellFeaturesManager.UpdateFeaturesAsync(features, [], force);

        return featuresToDisable;
    }

    public static async Task<bool> IsFeatureEnabledAsync(this IShellFeaturesManager shellFeaturesManager, string featureId)
    {
        ArgumentException.ThrowIfNullOrEmpty(featureId);

        var enabledFeatures = await shellFeaturesManager.GetEnabledFeaturesAsync();

        return enabledFeatures.Any(feature => feature.Id == featureId);
    }
}
