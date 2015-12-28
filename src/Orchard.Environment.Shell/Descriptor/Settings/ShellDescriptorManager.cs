using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell.Descriptor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using YesSql.Core.Services;

namespace Orchard.Environment.Shell.Descriptor.Settings
{
    public class ShellDescriptorManager : Component, IShellDescriptorManager
    {
        private readonly ShellSettings _shellSettings;
        //private readonly IEventNotifier _eventNotifier;
        private readonly ILogger _logger;
        private readonly ISession _session;

        public ShellDescriptorManager(
            ShellSettings shellSettings,
            //IEventNotifier eventNotifier,
            ILogger<ShellDescriptorManager> logger,
            ISession session)
        {
            _shellSettings = shellSettings;
            //_eventNotifier = eventNotifier;
            _session = session;
            _logger = logger;
        }

        public ShellDescriptor GetShellDescriptor()
        {
            // TODO: Load shell descriptor from database
            return null;
        }

        public void UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            var shellDescriptorRecord = GetShellDescriptor();
            var serialNumber = shellDescriptorRecord == null ? 0 : shellDescriptorRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
                throw new InvalidOperationException(T("Invalid serial number for shell descriptor").ToString());

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating shell descriptor for shell '{0}'...", _shellSettings.Name);
            }
            if (shellDescriptorRecord == null)
            {
                shellDescriptorRecord = new ShellDescriptor { SerialNumber = 1 };
                _session.Save(shellDescriptorRecord);
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
            //_eventNotifier.Notify<IShellDescriptorManagerEventHandler>(
            //    e => e.Changed(GetShellDescriptorFromRecord(shellDescriptorRecord), _shellSettings.Name));
        }
    }
}