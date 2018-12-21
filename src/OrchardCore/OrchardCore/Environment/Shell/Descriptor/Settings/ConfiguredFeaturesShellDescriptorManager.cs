using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Descriptor.Settings
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by returning the features from configuration.
    /// </summary>
    public class ConfiguredFeaturesShellDescriptorManager : IShellDescriptorManager
    {
        private readonly ShellSettings _shellSettings;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IEnumerable<ShellFeature> _alwaysEnabledFeatures;
        private ShellDescriptor _shellDescriptor;

        public ConfiguredFeaturesShellDescriptorManager(
            ShellSettings shellSettings,
            IShellSettingsManager shellSettingsManager,
            IEnumerable<ShellFeature> shellFeatures)
        {
            _shellSettings = shellSettings;
            _shellSettingsManager = shellSettingsManager;
            _alwaysEnabledFeatures = shellFeatures.Where(f => f.AlwaysEnabled).ToArray();
        }

        public Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            if (_shellDescriptor == null)
            {
                var configuredFeatures = new ConfiguredFeatures();
                _shellSettingsManager.Configuration.Bind("Tenants", configuredFeatures);
                _shellSettingsManager.Configuration.GetSection("Tenants").Bind(_shellSettings.Name, configuredFeatures);

                var features = _alwaysEnabledFeatures.Concat(configuredFeatures
                    .Features.Select(id => new ShellFeature(id))).Distinct();

                _shellDescriptor = new ShellDescriptor
                {
                    Features = features.ToList()
                };
            }

            return Task.FromResult(_shellDescriptor);
        }

        public Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            return Task.CompletedTask;
        }

        private class ConfiguredFeatures
        {
            public string[] Features { get; set; } = new string[] { };
        }
    }
}
