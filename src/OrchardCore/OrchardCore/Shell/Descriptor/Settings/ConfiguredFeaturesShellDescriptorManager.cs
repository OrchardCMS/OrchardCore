using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Descriptor.Settings
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by returning the features from configuration.
    /// </summary>
    public class ConfiguredFeaturesShellDescriptorManager : IShellDescriptorManager
    {
        private readonly IShellConfiguration _shellConfiguration;
        private readonly IEnumerable<ShellFeature> _alwaysEnabledFeatures;
        private readonly IExtensionManager _extensionManager;

        private ShellDescriptor _shellDescriptor;

        public ConfiguredFeaturesShellDescriptorManager(
            IShellConfiguration shellConfiguration,
            IEnumerable<ShellFeature> shellFeatures,
            IExtensionManager extensionManager)
        {
            _shellConfiguration = shellConfiguration;
            _alwaysEnabledFeatures = shellFeatures.Where(f => f.AlwaysEnabled).ToArray();
            _extensionManager = extensionManager;
        }

        public async Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            if (_shellDescriptor == null)
            {
                var configuredFeatures = new ConfiguredFeatures();
                _shellConfiguration.Bind(configuredFeatures);

                var features = _alwaysEnabledFeatures
                    .Concat(configuredFeatures.Features.Select(id => new ShellFeature(id) { AlwaysEnabled = true }))
                    .Distinct();

                var featureIds = features.Select(sf => sf.Id).ToArray();

                var missingDependencies = (await _extensionManager.LoadFeaturesAsync(featureIds))
                    .Select(entry => entry.FeatureInfo.Id)
                    .Except(featureIds)
                    .Select(id => new ShellFeature(id));

                _shellDescriptor = new ShellDescriptor
                {
                    Features = features
                        .Concat(missingDependencies)
                        .ToList()
                };
            }

            return _shellDescriptor;
        }

        public Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures)
        {
            return Task.CompletedTask;
        }

        private class ConfiguredFeatures
        {
            public string[] Features { get; set; } = Array.Empty<string>();
        }
    }
}
