using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Orchard.Environment.Shell
{
    public class ShellDescriptorFeaturesManager : IShellDescriptorFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        private readonly ILogger<ShellFeaturesManager> _logger;

        public ShellDescriptorFeaturesManager(IExtensionManager extensionManager,
            IShellDescriptorManager shellDescriptorManager,
            ILogger<ShellFeaturesManager> logger,
            IStringLocalizer<ShellFeaturesManager> localizer)
        {
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;

            _logger = logger;
            T = localizer;
        }
        public IStringLocalizer T { get; set; }

        public Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features)
        {
            return EnableFeaturesAsync(shellDescriptor, features, true);
        }

        public async Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features, bool force)
        {
            var featuresToEnable = features
                .SelectMany(feature => GetFeaturesToEnable(feature))
                .Distinct()
                .ToList();

            if (featuresToEnable.Count > 0 && force)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Forcibly enabling features {0}", string.Join(",", featuresToEnable.Select(x => x.Id)));
                }

                var enabledFeatures = await _extensionManager
                    .LoadFeaturesAsync(shellDescriptor.Features.Select(x => x.Id).ToArray());

                shellDescriptor.Features = enabledFeatures
                    .Select(x => x.FeatureInfo)
                    .Concat(featuresToEnable)
                    .Distinct()
                    .Select(x => new ShellFeature(x.Id))
                    .ToList();

                await _shellDescriptorManager.UpdateShellDescriptorAsync(
                    shellDescriptor.SerialNumber,
                    shellDescriptor.Features,
                    shellDescriptor.Parameters);
            }

            return featuresToEnable;
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        public Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features)
        {
            return DisableFeaturesAsync(shellDescriptor, features, true);
        }

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="features">The features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        public async Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features, bool force)
        {
            var featuresToDisable = features
                .SelectMany(feature => GetFeaturesToDisable(feature))
                .Distinct()
                .ToList();

            if (featuresToDisable.Count > 0 && force)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Forcibly disabling features {0}", string.Join(",", featuresToDisable.Select(x => x.Id)));
                }


                var leadedFeatures = await _extensionManager
                    .LoadFeaturesAsync(shellDescriptor.Features.Select(x => x.Id).ToArray());

                var enabledFeatures = leadedFeatures.Select(x => x.FeatureInfo).ToList();

                foreach (var feature in featuresToDisable)
                {
                    enabledFeatures.Remove(feature);

                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("{0} was disabled", feature.Id);
                    }
                }

                shellDescriptor.Features = enabledFeatures.Select(x => new ShellFeature(x.Id)).ToList();

                await _shellDescriptorManager.UpdateShellDescriptorAsync(
                    shellDescriptor.SerialNumber,
                    shellDescriptor.Features,
                    shellDescriptor.Parameters);
            }

            return featuresToDisable;
        }

        private IEnumerable<IFeatureInfo> GetFeaturesToEnable(IFeatureInfo featureInfo)
        {
            var featuresToEnable = _extensionManager
                .GetFeatureDependencies(featureInfo.Id)
                .ToList();

            return featuresToEnable;
        }

        
        private IEnumerable<IFeatureInfo> GetFeaturesToDisable(IFeatureInfo featureInfo)
        {
            var affectedFeatures = _extensionManager
                .GetDependentFeatures(featureInfo.Id)
                .ToList();

            return affectedFeatures;
        }
    }
}
