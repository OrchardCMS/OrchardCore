using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Data.Descriptors
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by providing the list of features store in the database.
    /// </summary>
    public class ShellDescriptorManager : IShellDescriptorManager
    {
        private readonly ShellSettings _shellSettings;
        private readonly IShellConfiguration _shellConfiguration;
        private readonly IEnumerable<ShellFeature> _alwaysEnabledFeatures;
        private readonly IEnumerable<IShellDescriptorManagerEventHandler> _shellDescriptorManagerEventHandlers;
        private readonly IExtensionManager _extensionManager;
        private readonly IDocumentStore _documentStore;
        private readonly ILogger _logger;

        private ShellDescriptor _shellDescriptor;

        public ShellDescriptorManager(
            ShellSettings shellSettings,
            IShellConfiguration shellConfiguration,
            IEnumerable<ShellFeature> shellFeatures,
            IEnumerable<IShellDescriptorManagerEventHandler> shellDescriptorManagerEventHandlers,
            IExtensionManager extensionManager,
            IDocumentStore documentStore,
            ILogger<ShellDescriptorManager> logger)
        {
            _shellSettings = shellSettings;
            _shellConfiguration = shellConfiguration;
            _alwaysEnabledFeatures = shellFeatures.Where(f => f.AlwaysEnabled).ToArray();
            _shellDescriptorManagerEventHandlers = shellDescriptorManagerEventHandlers;
            _extensionManager = extensionManager;
            _documentStore = documentStore;
            _logger = logger;
        }

        public async Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            if (_shellDescriptor != null)
            {
                return _shellDescriptor;
            }

            (var cacheable, var shellDescriptor) = await _documentStore.GetOrCreateImmutableAsync<ShellDescriptor>();

            if (shellDescriptor.SerialNumber == 0)
            {
                return null;
            }

            if (!cacheable)
            {
                // Clone ShellDescriptor
                shellDescriptor = new ShellDescriptor
                {
                    SerialNumber = shellDescriptor.SerialNumber,
                    Installed = new List<ShellFeature>(shellDescriptor.Installed),
                };
            }

            // Init shell descriptor and load features
            var configuredFeatures = new ConfiguredFeatures();
            _shellConfiguration.Bind(configuredFeatures);

            var features = _alwaysEnabledFeatures
                .Concat(configuredFeatures.Features.Select(id => new ShellFeature(id) { AlwaysEnabled = true }))
                .Concat(shellDescriptor.Features)
                .Distinct();

            var featureIds = features.Select(sf => sf.Id).ToArray();

            var missingDependencies = (await _extensionManager.LoadFeaturesAsync(featureIds))
                .Select(entry => entry.FeatureInfo.Id)
                .Except(featureIds)
                .Select(id => new ShellFeature(id));

            shellDescriptor.Features = features
                .Concat(missingDependencies)
                .ToList();

            return _shellDescriptor = shellDescriptor;
        }

        public async Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures)
        {
            var shellDescriptor = await _documentStore.GetOrCreateMutableAsync<ShellDescriptor>();

            if (priorSerialNumber != shellDescriptor.SerialNumber)
            {
                throw new InvalidOperationException("Invalid serial number for shell descriptor");
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating shell descriptor for tenant '{TenantName}' ...", _shellSettings.Name);
            }

            shellDescriptor.SerialNumber++;
            shellDescriptor.Features = _alwaysEnabledFeatures.Union(enabledFeatures).ToList();
            shellDescriptor.Installed = shellDescriptor.Installed.Union(shellDescriptor.Features).ToList();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Shell descriptor updated for tenant '{TenantName}'.", _shellSettings.Name);
            }

            await _documentStore.UpdateAsync(shellDescriptor, _ => Task.CompletedTask);
            await _documentStore.CommitAsync();

            // In the 'ChangedAsync()' event the shell will be released so that, on request, a new one will be built.
            // So, we commit the session earlier to prevent a new shell from being built from an outdated descriptor.
            await _shellDescriptorManagerEventHandlers.InvokeAsync((handler, shellDescriptor, _shellSettings) =>
                handler.ChangedAsync(shellDescriptor, _shellSettings), shellDescriptor, _shellSettings, _logger);
        }

        private class ConfiguredFeatures
        {
            public string[] Features { get; set; } = Array.Empty<string>();
        }
    }
}
