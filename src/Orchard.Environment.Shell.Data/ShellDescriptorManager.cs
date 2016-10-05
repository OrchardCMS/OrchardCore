using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Events;
using YesSql.Core.Services;

namespace Orchard.Environment.Shell.Data.Descriptors
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by providing the list of features store in the database. 
    /// </summary>
    public class ShellDescriptorManager : IShellDescriptorManager
    {
        private readonly ShellSettings _shellSettings;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly ILogger _logger;
        private ShellDescriptor _shellDescriptor;

        public ShellDescriptorManager(
            ShellSettings shellSettings,
            IEventBus eventBus,
            ISession session,
            ILogger<ShellDescriptorManager> logger)
        {
            _shellSettings = shellSettings;
            _eventBus = eventBus;
            _session = session;
            _logger = logger;
        }

        public async Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            // Prevent multiple queries during the same request
            if (_shellDescriptor == null)
            {
                _shellDescriptor = await _session.QueryAsync<ShellDescriptor>().FirstOrDefault();
            }

            return _shellDescriptor;
        }

        public async Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            var shellDescriptorRecord = await GetShellDescriptorAsync();
            var serialNumber = shellDescriptorRecord == null ? 0 : shellDescriptorRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
            {
                throw new InvalidOperationException("Invalid serial number for shell descriptor");
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating shell descriptor for shell '{0}'...", _shellSettings.Name);
            }

            if (shellDescriptorRecord == null)
            {
                shellDescriptorRecord = new ShellDescriptor { SerialNumber = 1 };
            }
            else
            {
                shellDescriptorRecord.SerialNumber++;
            }

            shellDescriptorRecord.Features.Clear();

            foreach (var feature in enabledFeatures)
            {
                shellDescriptorRecord.Features.Add(new ShellFeature { Name = feature.Name });
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Enabled features for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", enabledFeatures.Select(feature => feature.Name)));
            }

            shellDescriptorRecord.Parameters.Clear();

            foreach (var parameter in parameters)
            {
                shellDescriptorRecord.Parameters.Add(new ShellParameter
                {
                    Component = parameter.Component,
                    Name = parameter.Name,
                    Value = parameter.Value
                });
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Parameters for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", parameters.Select(parameter => parameter.Name + "-" + parameter.Value)));
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {

                _logger.LogInformation("Shell descriptor updated for shell '{0}'.", _shellSettings.Name);
            }

            _session.Save(shellDescriptorRecord);

            // Update cached reference
            _shellDescriptor = shellDescriptorRecord;

            await _eventBus.NotifyAsync<IShellDescriptorManagerEventHandler>(e => e.Changed(shellDescriptorRecord, _shellSettings.Name));
        }
    }
}