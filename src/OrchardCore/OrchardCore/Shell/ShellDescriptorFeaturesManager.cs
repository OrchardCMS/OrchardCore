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
            IEnumerable<IFeatureInfo> featuresToDisable, IEnumerable<IFeatureInfo> featuresToEnable, bool force)
        {
            var allOrderedFeatures = _extensionManager.GetFeatures();

            var enabledFeatures = allOrderedFeatures
                .Where(f => shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                .ToList();

            var alwaysEnabledIds = _alwaysEnabledFeatures.Select(sf => sf.Id).ToArray();
            var enabledFeatureIds = enabledFeatures.Select(f => f.Id).ToArray();

            var allFeaturesToDisable = featuresToDisable
                .Where(f => !alwaysEnabledIds.Contains(f.Id))
                .SelectMany(feature => GetFeaturesToDisable(feature, enabledFeatureIds, force))
                .Distinct()
                .Reverse()
                .ToList();

            foreach (var feature in allFeaturesToDisable)
            {
                enabledFeatures.Remove(feature);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Feature '{FeatureName}' was disabled", feature.Id);
                }
            }

            enabledFeatureIds = enabledFeatures.Select(f => f.Id).ToArray();

            var allFeaturesToEnable = featuresToEnable
                .SelectMany(feature => GetFeaturesToEnable(feature, enabledFeatureIds, force))
                .Distinct()
                .ToList();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                foreach (var feature in allFeaturesToEnable)
                {
                    _logger.LogInformation("Enabling feature '{FeatureName}'", feature.Id);
                }
            }

            var installedFeatureIds = shellDescriptor.Installed.Select(sf => sf.Id).ToArray();

            var allFeaturesToInstall = allFeaturesToEnable
                .Where(f => !installedFeatureIds.Contains(f.Id))
                .ToList();

            if (allFeaturesToEnable.Count > 0)
            {
                enabledFeatures = enabledFeatures.Concat(allFeaturesToEnable).Distinct().ToList();

                installedFeatureIds = installedFeatureIds
                    .Concat(allFeaturesToInstall.Select(f => f.Id))
                    .ToArray();
            }

            if (allFeaturesToDisable.Count > 0 || allFeaturesToEnable.Count > 0)
            {
                var enabledShellFeatures = enabledFeatures.Select(f => new ShellFeature(f.Id)).ToList();

                var installedShellFeatures = allOrderedFeatures
                    .Where(f => !installedFeatureIds.Contains(f.Id))
                    .Select(x => new ShellFeature(x.Id))
                    .ToList();

                await _shellDescriptorManager.UpdateShellDescriptorAsync(
                    shellDescriptor.SerialNumber,
                    enabledShellFeatures,
                    installedShellFeatures);

                ShellScope.AddDeferredTask(async scope =>
                {
                    var featureEventHandlers = scope.ServiceProvider.GetServices<IFeatureEventHandler>();

                    foreach (var feature in allFeaturesToInstall)
                    {
                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.InstallingAsync(featureInfo), feature, _logger);
                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.InstalledAsync(featureInfo), feature, _logger);
                    }

                    foreach (var feature in allFeaturesToEnable)
                    {
                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.EnablingAsync(featureInfo), feature, _logger);
                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.EnabledAsync(featureInfo), feature, _logger);
                    }

                    foreach (var feature in allFeaturesToDisable)
                    {
                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.DisablingAsync(featureInfo), feature, _logger);
                        await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.DisabledAsync(featureInfo), feature, _logger);
                    }
                });
            }

            return (allFeaturesToDisable, allFeaturesToEnable);
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
                .Where(f => !enabledFeatureIds.Contains(f.Id))
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
                .Where(f => enabledFeatureIds.Contains(f.Id))
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
