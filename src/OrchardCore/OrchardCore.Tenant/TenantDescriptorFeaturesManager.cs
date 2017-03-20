using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Extensions;
using OrchardCore.Extensions.Features;
using OrchardCore.Tenant.Descriptor;
using OrchardCore.Tenant.Descriptor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Tenant
{
    public class TenantDescriptorFeaturesManager : ITenantDescriptorFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly ITenantDescriptorManager _tenantDescriptorManager;

        private readonly ILogger<TenantFeaturesManager> _logger;

        public FeatureDependencyNotificationHandler FeatureDependencyNotification { get; set; }

        public TenantDescriptorFeaturesManager(IExtensionManager extensionManager,
            ITenantDescriptorManager tenantDescriptorManager,
            ILogger<TenantFeaturesManager> logger,
            IStringLocalizer<TenantFeaturesManager> localizer)
        {
            _extensionManager = extensionManager;
            _tenantDescriptorManager = tenantDescriptorManager;

            _logger = logger;
            T = localizer;
        }
        public IStringLocalizer T { get; set; }

        public Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(TenantDescriptor tenantDescriptor, IEnumerable<IFeatureInfo> features)
        {
            return EnableFeaturesAsync(tenantDescriptor, features, false);
        }

        public async Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(TenantDescriptor tenantDescriptor, IEnumerable<IFeatureInfo> features, bool force)
        {
            var featuresToEnable = features
                .SelectMany(feature => GetFeaturesToEnable(feature, false))
                .Distinct()
                .ToList();

            if (featuresToEnable.Count > 0)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Enabling features {0}", string.Join(",", featuresToEnable.Select(x => x.Id)));
                }

                var enabledFeatures = await _extensionManager
                    .LoadFeaturesAsync(tenantDescriptor.Features.Select(x => x.Id).ToArray());

                tenantDescriptor.Features = enabledFeatures
                    .Select(x => x.FeatureInfo)
                    .Concat(featuresToEnable)
                    .Distinct()
                    .Select(x => new TenantFeature(x.Id))
                    .ToList();

                await _tenantDescriptorManager.UpdateTenantDescriptorAsync(
                    tenantDescriptor.SerialNumber,
                    tenantDescriptor.Features,
                    tenantDescriptor.Parameters);
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        public Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(TenantDescriptor tenantDescriptor, IEnumerable<IFeatureInfo> features)
        {
            return DisableFeaturesAsync(tenantDescriptor, features, false);
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="features">The features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        public async Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(TenantDescriptor tenantDescriptor, IEnumerable<IFeatureInfo> features, bool force)
        {
            var featuresToDisable = features
                .SelectMany(feature => GetFeaturesToDisable(feature, force))
                .Distinct()
                .ToList();

            if (featuresToDisable.Count > 0)
            {
                var leadedFeatures = await _extensionManager
                    .LoadFeaturesAsync(tenantDescriptor.Features.Select(x => x.Id).ToArray());

                var enabledFeatures = leadedFeatures.Select(x => x.FeatureInfo).ToList();

                foreach (var feature in featuresToDisable)
                {
                    enabledFeatures.Remove(feature);

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("{0} was disabled", feature.Id);
                    }
                }

                tenantDescriptor.Features = enabledFeatures.Select(x => new TenantFeature(x.Id)).ToList();

                await _tenantDescriptorManager.UpdateTenantDescriptorAsync(
                    tenantDescriptor.SerialNumber,
                    tenantDescriptor.Features,
                    tenantDescriptor.Parameters);
            }

            return featuresToDisable;
        }

        /// <summary>
        /// Enables a feature.
        /// </summary>
        /// <param name="featureId">The ID of the feature to be enabled.</param>
        /// <param name="availableFeatures">A dictionary of the available feature descriptors and their current state (enabled / disabled).</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration of the enabled features.</returns>
        private IEnumerable<IFeatureInfo> GetFeaturesToEnable(
            IFeatureInfo featureInfo,
            bool force)
        {
            var featuresToEnable = _extensionManager
                .GetFeatureDependencies(featureInfo.Id)
                .ToList();

            if (featuresToEnable.Count > 1 && !force)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Additional features need to be enabled.");
                }
                if (FeatureDependencyNotification != null)
                {
                    FeatureDependencyNotification("If {0} is enabled, then you'll also need to enable {1}.", featureInfo, featuresToEnable.Where(f => f.Id != featureInfo.Id));
                }
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a feature.
        /// </summary>
        /// <param name="featureId">The ID of the feature to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration of the disabled features.</returns>
        private IEnumerable<IFeatureInfo> GetFeaturesToDisable(IFeatureInfo featureInfo, bool force)
        {
            var affectedFeatures = _extensionManager
                .GetDependentFeatures(featureInfo.Id)
                .ToList();

            if (affectedFeatures.Count > 1 && !force)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("Additional features need to be disabled.");
                }
                if (FeatureDependencyNotification != null)
                {
                    FeatureDependencyNotification("If {0} is disabled, then you'll also need to disable {1}.", featureInfo, affectedFeatures.Where(f => f.Id != featureInfo.Id));
                }
            }

            return affectedFeatures;
        }
    }
}
