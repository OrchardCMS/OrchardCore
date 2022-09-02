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
            var featureEventHandlers = ShellScope.Services.GetServices<IFeatureEventHandler>();

            var enabledFeatures = _extensionManager.GetFeatures()
                .Where(f => shellDescriptor.Features.Any(sf => sf.Id == f.Id))
                .ToArray();

            var enabledFeatureIds = enabledFeatures
                .Select(f => f.Id)
                .ToHashSet();

            var installedFeatureIds = enabledFeatureIds
                .Concat(shellDescriptor.Installed.Select(sf => sf.Id))
                .ToHashSet();

            var alwaysEnabledIds = _alwaysEnabledFeatures.Select(sf => sf.Id).ToArray();

            var byDependencyOnlyFeaturesToDisable = enabledFeatures
                .Where(f => f.EnabledByDependencyOnly);

            var allFeaturesToDisable = featuresToDisable
                .Where(f => !alwaysEnabledIds.Contains(f.Id))
                .SelectMany(feature => GetFeaturesToDisable(feature, enabledFeatureIds, force))
                // Always attempt to disable 'EnabledByDependencyOnly' features
                // to ensure we auto disable any feature that is no longer needed.
                .Union(byDependencyOnlyFeaturesToDisable)
                .Distinct()
                .Reverse()
                .ToList();

            foreach (var feature in allFeaturesToDisable)
            {
                enabledFeatureIds.Remove(feature.Id);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Disabling feature '{FeatureName}'", feature.Id);
                }

                await featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.DisablingAsync(featureInfo), feature, _logger);
            }

            var allFeaturesToEnable = featuresToEnable
                .SelectMany(feature => GetFeaturesToEnable(feature, enabledFeatureIds, force))
                .Distinct()
                .ToList();

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
