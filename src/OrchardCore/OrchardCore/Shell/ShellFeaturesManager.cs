using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell
{
    public class ShellFeaturesManager : IShellFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IShellDescriptorFeaturesManager _shellDescriptorFeaturesManager;
        private readonly IEnumerable<IFeatureValidationProvider> _featureValidators;

        public ShellFeaturesManager(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IShellDescriptorFeaturesManager shellDescriptorFeaturesManager,
            IEnumerable<IFeatureValidationProvider> featureValidators)
        {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _shellDescriptorFeaturesManager = shellDescriptorFeaturesManager;
            _featureValidators = featureValidators;
        }

        public async Task<IEnumerable<IFeatureInfo>> GetAvailableFeaturesAsync()
        {
            var features = _extensionManager.GetFeatures();
            var result = new List<IFeatureInfo>();
            foreach (var feature in features)
            {
                var isFeatureValid = true;
                foreach (var validator in _featureValidators)
                {
                    isFeatureValid = await validator.IsFeatureValidAsync(feature.Id);
                    // When a feature is marked as invalid it cannot be reintroduced.
                    if (!isFeatureValid)
                    {
                        break;
                    }
                }

                if (isFeatureValid)
                {
                    result.Add(feature);
                }
            }

            return result;
        }

        public Task<IEnumerable<IFeatureInfo>> GetEnabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => _shellDescriptor.Features.Any(sf => sf.Id == f.Id)));
        }

        public Task<IEnumerable<IFeatureInfo>> GetAlwaysEnabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => f.IsAlwaysEnabled || _shellDescriptor.Features.Any(sf => sf.Id == f.Id && sf.AlwaysEnabled)));
        }

        public Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => _shellDescriptor.Features.All(sf => sf.Id != f.Id)));
        }

        public async Task<(IEnumerable<IFeatureInfo>, IEnumerable<IFeatureInfo>)> UpdateFeaturesAsync(IEnumerable<IFeatureInfo> featuresToDisable, IEnumerable<IFeatureInfo> featuresToEnable, bool force)
        {
            var toDisable = new List<IFeatureInfo>();
            var toEnable = new List<IFeatureInfo>();

            // Look for features that can be enabled automatically.
            foreach (var featureToEnable in featuresToEnable)
            {
                if (!featureToEnable.EnabledByDependencyOnly)
                {
                    // EnabledByDependencyOnly features are managed internally and can't be explicitly enabled.
                    continue;
                }

                // When any of the dependencies are EnabledByDependencyOnly, we'll need to manually enable them.
                var autoEnable = _extensionManager.GetFeatureDependencies(featureToEnable.Id)
                    .Where(feature => !feature.EnabledByDependencyOnly).ToArray();

                toEnable.AddRange(autoEnable);
                toEnable.Add(featureToEnable);
            }

            // Look for features that can be disabled automatically.
            foreach (var featureToDisable in featuresToDisable)
            {
                if (!featureToDisable.EnabledByDependencyOnly)
                {
                    // EnabledByDependencyOnly features are managed internally and can't be explicitly disabled
                    continue;
                }
                // If any of the dependencies are EnabledByDependencyOnly, we'll need to manually disable them.
                // Get any features that are selectable and could be disabled automatically.
                var canBeDisabled = _extensionManager.GetFeatureDependencies(featureToDisable.Id)
                    .Where(feature => !feature.EnabledByDependencyOnly && !toEnable.Any(willEnableFeature => willEnableFeature.Id == feature.Id))
                    .ToArray();

                toDisable.AddRange(canBeDisabled);
                toDisable.Add(featureToDisable);
            }

            var willAutoDisable = toDisable.Where(f => !f.EnabledByDependencyOnly).ToArray();

            if (willAutoDisable.Length > 0)
            {
                // At this point, we have features that will be auto disabled.
                // Let's make sure no enabled features will still need them.
                var enabledFeatures = await GetEnabledFeaturesAsync();

                // At this point, we know there are at least one feature that will be automatically disabled.
                foreach (var enabledFeature in enabledFeatures)
                {
                    if (toDisable.Any(feature => feature.Id == enabledFeature.Id && enabledFeature.EnabledByDependencyOnly))
                    {
                        // This feature will be disabled, we don't need to evaluate it
                        continue;
                    }

                    ShouldDisable(enabledFeature.Id, toDisable, willAutoDisable);
                }
            }

            return await _shellDescriptorFeaturesManager.UpdateFeaturesAsync(_shellDescriptor, toDisable, toEnable, force);
        }

        public Task<IEnumerable<IExtensionInfo>> GetEnabledExtensionsAsync()
        {
            // enabled extensions are those which have at least one enabled feature.
            var enabledIds = _extensionManager.GetFeatures().Where(f => _shellDescriptor
                .Features.Any(sf => sf.Id == f.Id)).Select(f => f.Extension.Id).Distinct().ToArray();

            // Extensions are still ordered according to the weight of their first features.
            return Task.FromResult(_extensionManager.GetExtensions().Where(e => enabledIds.Contains(e.Id)));
        }

        /// <summary>
        /// Recursively evaluate dependencies and removes it necessary from toDisable
        /// </summary>
        private void ShouldDisable(string id, List<IFeatureInfo> toDisable, IFeatureInfo[] willAutoDisable)
        {
            var dependencies = _extensionManager.GetFeatureDependencies(id);

            foreach (var dependency in dependencies)
            {
                if (dependency.Id == id || dependency.EnabledByDependencyOnly)
                {
                    continue;
                }

                var cannotDisable = willAutoDisable.FirstOrDefault(feature => feature.Id == dependency.Id);

                if (cannotDisable != null)
                {
                    // This dependency is needed by other features, let's not disable it.
                    toDisable.Remove(cannotDisable);
                }
                else
                {
                    ShouldDisable(dependency.Id, toDisable, willAutoDisable);
                }
            }
        }
    }
}
