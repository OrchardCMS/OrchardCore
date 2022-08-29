using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell
{
    public class ShellDescriptorFeaturesManager : IShellDescriptorFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<ShellFeature> _alwaysEnabledFeatures;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly ILogger _logger;

        public ShellDescriptorFeaturesManager(
            IExtensionManager extensionManager,
            IEnumerable<ShellFeature> shellFeatures,
            IShellDescriptorManager shellDescriptorManager,
            ILogger<ShellFeaturesManager> logger)
        {
            _extensionManager = extensionManager;
            _alwaysEnabledFeatures = shellFeatures.Where(f => f.AlwaysEnabled).ToArray();
            _shellDescriptorManager = shellDescriptorManager;
            _logger = logger;
        }

        public async Task<(IEnumerable<IFeatureInfo>, IEnumerable<IFeatureInfo>)> UpdateFeaturesAsync(ShellDescriptor shellDescriptor,
            IEnumerable<IFeatureInfo> featuresToDisable,
            IEnumerable<IFeatureInfo> featuresToEnable,
            bool force)
        {
            var featureEventHandlers = ShellScope.Services.GetServices<IFeatureEventHandler>();

            var enabledFeatures = _extensionManager.GetFeatures()
                .Where(f => shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                .ToHashSet();

            var enabledFeatureIds = enabledFeatures.Select(f => f.Id)
                .ToHashSet();

            var installedFeatureIds = enabledFeatureIds
                .Concat(shellDescriptor.Installed.Select(sf => sf.Id))
                .ToHashSet();

            var alwaysEnabledIds = _alwaysEnabledFeatures.Select(sf => sf.Id).ToArray();

            var safeToEnableFeatures = new List<IFeatureInfo>();

            // Look for features that can be enabled automatically.
            foreach (var featureToEnable in featuresToEnable)
            {
                if (featureToEnable.EnabledByDependencyOnly)
                {
                    // EnabledByDependencyOnly features are managed internally and can't be explicitly enabled.
                    continue;
                }

                // its safe to enable this feature
                safeToEnableFeatures.AddRange(GetFeaturesToEnable(featureToEnable, enabledFeatureIds, force));

                // Find any dependency that has EnabledByDependencyOnly, and manually enable it.
                var autoEnable = _extensionManager.GetFeatureDependencies(featureToEnable.Id)
                    .Where(feature => feature.EnabledByDependencyOnly);

                safeToEnableFeatures.AddRange(autoEnable);
            }

            var safeToDisableFeatures = new List<IFeatureInfo>();

            // Look for features that can be disabled automatically.
            foreach (var featureToDisable in featuresToDisable)
            {
                if (featureToDisable.IsAlwaysEnabled || featureToDisable.EnabledByDependencyOnly)
                {
                    // IsAlwaysEnabled cannot be disabled
                    // EnabledByDependencyOnly features are managed internally and can't be explicitly disabled
                    continue;
                }

                // It's safe to disable this feature
                safeToDisableFeatures.AddRange(GetFeaturesToDisable(featureToDisable, enabledFeatureIds, force));

                // If any of the dependencies has EnabledByDependencyOnly, we'll need to manually disable it.
                // Get any features that are selectable and could be disabled automatically.
                var canBeDisabled = _extensionManager.GetFeatureDependencies(featureToDisable.Id)
                    .Where(feature => feature.EnabledByDependencyOnly && !safeToEnableFeatures.Any(safeToEnableFeature => safeToEnableFeature.Id == feature.Id));

                safeToDisableFeatures.AddRange(canBeDisabled);
            }

            var allFeaturesToDisable = safeToDisableFeatures.Distinct().Reverse().ToList();

            var willAutoDisable = safeToDisableFeatures.Where(safeToDisableFeature => safeToDisableFeature.EnabledByDependencyOnly).ToArray();

            if (willAutoDisable.Length > 0)
            {
                // At this point, we know there are at least one feature that will be automatically disabled.

                // Let's check all the enabled features recursively to make sure it's truely safe to disable them
                foreach (var enabledFeature in enabledFeatures)
                {
                    if (safeToDisableFeatures.Any(feature => feature.Id == enabledFeature.Id && !enabledFeature.EnabledByDependencyOnly))
                    {
                        // This feature will be disabled, we don't need to evaluate it
                        continue;
                    }

                    EvaluateDependenciesAndKeepWhatIsNeeded(enabledFeature.Id, safeToDisableFeatures, willAutoDisable);
                }
            }

            foreach (var feature in allFeaturesToDisable)
            {
                enabledFeatureIds.Remove(feature.Id);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Disabling feature '{FeatureName}'", feature.Id);
                }

                await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.DisablingAsync(featureInfo), feature, _logger);
            }

            var allFeaturesToEnable = safeToEnableFeatures.Distinct().ToList();

            foreach (var feature in allFeaturesToEnable)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Enabling feature '{FeatureName}'", feature.Id);
                }

                await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.EnablingAsync(featureInfo), feature, _logger);
            }

            var allFeaturesToInstall = allFeaturesToEnable
                .Where(f => !installedFeatureIds.Contains(f.Id))
                .ToList();

            foreach (var feature in allFeaturesToInstall)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Installing feature '{FeatureName}'", feature.Id);
                }

                await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.InstallingAsync(featureInfo), feature, _logger);
            }

            if (allFeaturesToEnable.Count > 0)
            {
                enabledFeatureIds.UnionWith(allFeaturesToEnable.Select(f => f.Id));
            }

            if (allFeaturesToDisable.Count > 0 || allFeaturesToEnable.Count > 0)
            {
                await _shellDescriptorManager.UpdateShellDescriptorAsync(
                    shellDescriptor.SerialNumber,
                    enabledFeatureIds.Select(id => new ShellFeature(id)).ToArray());

                ShellScope.AddDeferredTask(async scope =>
                {
                    var featureEventHandlers = scope.ServiceProvider.GetServices<IFeatureEventHandler>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ShellFeaturesManager>>();

                    foreach (var feature in allFeaturesToInstall)
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("Feature '{FeatureName}' was installed", feature.Id);
                        }

                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.InstalledAsync(featureInfo), feature, logger);
                    }

                    foreach (var feature in allFeaturesToEnable)
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("Feature '{FeatureName}' was enabled", feature.Id);
                        }

                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.EnabledAsync(featureInfo), feature, logger);
                    }

                    foreach (var feature in allFeaturesToDisable)
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("Feature '{FeatureName}' was disabled", feature.Id);
                        }

                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.DisabledAsync(featureInfo), feature, logger);
                    }
                });
            }

            return (allFeaturesToDisable, allFeaturesToEnable);
        }

        /// <summary>
        /// Evaluate dependencies and removes it necessary from toDisable
        /// </summary>
        private void EvaluateDependenciesAndKeepWhatIsNeeded(string id, List<IFeatureInfo> safeToDisableFeatures, IFeatureInfo[] willAutoDisable)
        {
            var features = _extensionManager.GetFeatureDependencies(id)
                .Union(_extensionManager.GetDependentFeatures(id));

            foreach (var feature in features)
            {
                if (feature.Id == id || !feature.EnabledByDependencyOnly)
                {
                    continue;
                }

                var cannotDisable = willAutoDisable.FirstOrDefault(feature => feature.Id == feature.Id);

                if (cannotDisable != null)
                {
                    // This dependency is needed by other features, let's not disable it.
                    safeToDisableFeatures.Remove(cannotDisable);
                }
            }
        }

        /// <summary>
        /// Enables a feature.
        /// </summary>
        /// <param name="featureInfo">The info of the feature to be enabled.</param>
        /// <param name="enabledFeatureIds">The list of feature ids which are currently enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies.</param>
        /// <returns>An enumeration of the features to disable, empty if 'force' = true and a dependency is disabled</returns>
        private IEnumerable<IFeatureInfo> GetFeaturesToEnable(IFeatureInfo featureInfo, IEnumerable<string> enabledFeatureIds, bool force)
        {
            var featuresToEnable = _extensionManager
                .GetFeatureDependencies(featureInfo.Id)
                .Where(f => !f.EnabledByDependencyOnly && !enabledFeatureIds.Contains(f.Id))
                .ToList();

            if (featuresToEnable.Count > 1 && !force)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(" To enable '{FeatureId}', additional features need to be enabled.", featureInfo.Id);
                }

                return Enumerable.Empty<IFeatureInfo>();
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a feature.
        /// </summary>
        /// <param name="featureInfo">The info of the feature to be disabled.</param>
        /// <param name="enabledFeatureIds">The list of feature ids which are currently enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable it's dependents.</param>
        /// <returns>An enumeration of the features to enable, empty if 'force' = true and a dependent is enabled</returns>
        private IEnumerable<IFeatureInfo> GetFeaturesToDisable(IFeatureInfo featureInfo, IEnumerable<string> enabledFeatureIds, bool force)
        {
            var featuresToDisable = _extensionManager
                .GetDependentFeatures(featureInfo.Id)
                .Where(f => !f.EnabledByDependencyOnly && enabledFeatureIds.Contains(f.Id))
                .ToList();

            if (featuresToDisable.Count > 1 && !force)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(" To disable '{FeatureId}', additional features need to be disabled.", featureInfo.Id);
                }

                return Enumerable.Empty<IFeatureInfo>();
            }

            return featuresToDisable;
        }
    }
}
