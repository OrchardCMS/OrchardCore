using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Descriptor.Models;
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
            ILoggerFactory loggerFactory,
            ISession session)
        {
            _shellSettings = shellSettings;
            //_eventNotifier = eventNotifier;
            _session = session;
            _logger = loggerFactory.CreateLogger<ShellDescriptorManager>();
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

            _logger.LogInformation("Updating shell descriptor for shell '{0}'...", _shellSettings.Name);

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
            _logger.LogDebug("Enabled features for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", enabledFeatures.Select(feature => feature.Name)));


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
            _logger.LogDebug("Parameters for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", parameters.Select(parameter => parameter.Name + "-" + parameter.Value)));

            _logger.LogInformation("Shell descriptor updated for shell '{0}'.", _shellSettings.Name);

            //_eventNotifier.Notify<IShellDescriptorManagerEventHandler>(
            //    e => e.Changed(GetShellDescriptorFromRecord(shellDescriptorRecord), _shellSettings.Name));
        }
    }
}