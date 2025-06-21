using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Features.Models;
using OrchardCore.Features.ViewModels;

namespace OrchardCore.Features.Services;

public class FeatureService
{
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IExtensionManager _extensionManager;

    public FeatureService(IShellFeaturesManager shellFeaturesManager, IExtensionManager extensionManager)
    {
        _shellFeaturesManager = shellFeaturesManager;
        _extensionManager = extensionManager;
    }

    public async Task<IEnumerable<IFeatureInfo>> GetAvailableFeatures()
    {
        return (await _shellFeaturesManager.GetAvailableFeaturesAsync().ConfigureAwait(false)).Where(feature => !feature.IsTheme());
    }

    public async Task<IFeatureInfo> GetAvailableFeature(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return (await GetAvailableFeatures().ConfigureAwait(false)).FirstOrDefault(feature => feature.Id == id);
    }

    public async Task<IEnumerable<IFeatureInfo>> GetAvailableFeatures(string[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return [];
        }

        return (await GetAvailableFeatures().ConfigureAwait(false)).Where(feature => ids.Contains(feature.Id));
    }

    public async Task<IEnumerable<ModuleFeature>> GetModuleFeaturesAsync()
    {
        var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync().ConfigureAwait(false);
        var alwaysEnabledFeatures = await _shellFeaturesManager.GetAlwaysEnabledFeaturesAsync().ConfigureAwait(false);

        var moduleFeatures = new List<ModuleFeature>();

        foreach (var moduleFeatureInfo in await GetAvailableFeatures().ConfigureAwait(false))
        {
            var dependentFeatures = _extensionManager.GetDependentFeatures(moduleFeatureInfo.Id);
            var featureDependencies = _extensionManager.GetFeatureDependencies(moduleFeatureInfo.Id);

            var moduleFeature = new ModuleFeature
            {
                Descriptor = moduleFeatureInfo,
                IsEnabled = enabledFeatures.Contains(moduleFeatureInfo),
                EnabledByDependencyOnly = moduleFeatureInfo.EnabledByDependencyOnly,
                IsAlwaysEnabled = alwaysEnabledFeatures.Contains(moduleFeatureInfo),
                EnabledDependentFeatures = dependentFeatures.Where(x => x.Id != moduleFeatureInfo.Id && enabledFeatures.Contains(x)).ToList(),
                FeatureDependencies = featureDependencies.Where(d => d.Id != moduleFeatureInfo.Id).ToList(),
            };

            moduleFeatures.Add(moduleFeature);
        }

        return moduleFeatures;
    }

    public async Task EnableOrDisableFeaturesAsync(IEnumerable<IFeatureInfo> features, FeaturesBulkAction action, bool? force, Func<IEnumerable<IFeatureInfo>, bool, Task> notifyAsync)
    {
        switch (action)
        {
            case FeaturesBulkAction.Enable:
                await _shellFeaturesManager.EnableFeaturesAsync(features, force == true).ConfigureAwait(false);
                await notifyAsync(features, true).ConfigureAwait(false);
                break;
            case FeaturesBulkAction.Disable:
                await _shellFeaturesManager.DisableFeaturesAsync(features, force == true).ConfigureAwait(false);
                await notifyAsync(features, false).ConfigureAwait(false);
                break;
            case FeaturesBulkAction.Toggle:
                // The features array has already been checked for validity.
                var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync().ConfigureAwait(false);
                var disabledFeatures = await _shellFeaturesManager.GetDisabledFeaturesAsync().ConfigureAwait(false);
                var featuresToEnable = disabledFeatures.Intersect(features);
                var featuresToDisable = enabledFeatures.Intersect(features);

                await _shellFeaturesManager.UpdateFeaturesAsync(featuresToDisable, featuresToEnable, force == true).ConfigureAwait(false);
                await notifyAsync(featuresToEnable, true).ConfigureAwait(false);
                await notifyAsync(featuresToDisable, false).ConfigureAwait(false);
                return;
            default:
                break;
        }
    }
}
