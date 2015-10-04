using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Descriptor.Settings.Records;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Orchard.Environment.Shell.Descriptor.Settings {
    public class ShellDescriptorManager : Component, IShellDescriptorManager {
        private readonly IContentStorageManager _contentStorageManager;
        private readonly ShellSettings _shellSettings;
        //private readonly IEventNotifier _eventNotifier;
        private readonly ILogger _logger;

        public ShellDescriptorManager(
            IContentStorageManager contentStorageManager,
            ShellSettings shellSettings,
            //IEventNotifier eventNotifier,
            ILoggerFactory loggerFactory) {
            _contentStorageManager = contentStorageManager;
            _shellSettings = shellSettings;
            //_eventNotifier = eventNotifier;
            _logger = loggerFactory.CreateLogger<ShellDescriptorManager>();
        }

        public ShellDescriptor GetShellDescriptor() {
            ShellDescriptorRecord shellDescriptorRecord = GetDescriptorRecord();
            if (shellDescriptorRecord == null) return null;
            return GetShellDescriptorFromRecord(shellDescriptorRecord);
        }

        private static ShellDescriptor GetShellDescriptorFromRecord(ShellDescriptorRecord shellDescriptorRecord) {
            ShellDescriptor descriptor = new ShellDescriptor { SerialNumber = shellDescriptorRecord.SerialNumber };
            var descriptorFeatures = new List<ShellFeature>();
            foreach (var descriptorFeatureRecord in shellDescriptorRecord.Features) {
                descriptorFeatures.Add(new ShellFeature { Name = descriptorFeatureRecord.Name });
            }
            descriptor.Features = descriptorFeatures;
            var descriptorParameters = new List<ShellParameter>();
            foreach (var descriptorParameterRecord in shellDescriptorRecord.Parameters) {
                descriptorParameters.Add(
                    new ShellParameter {
                        Component = descriptorParameterRecord.Component,
                        Name = descriptorParameterRecord.Name,
                        Value = descriptorParameterRecord.Value
                    });
            }
            descriptor.Parameters = descriptorParameters;

            return descriptor;
        }

        private ShellDescriptorRecord GetDescriptorRecord() {
            return _contentStorageManager.Query<ShellDescriptorRecord>(x => x != null).FirstOrDefault();
        }

        public void UpdateShellDescriptor(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters) {
            ShellDescriptorRecord shellDescriptorRecord = GetDescriptorRecord();
            var serialNumber = shellDescriptorRecord == null ? 0 : shellDescriptorRecord.SerialNumber;
            if (priorSerialNumber != serialNumber)
                throw new InvalidOperationException(T("Invalid serial number for shell descriptor").ToString());

            _logger.LogInformation("Updating shell descriptor for shell '{0}'...", _shellSettings.Name);

            if (shellDescriptorRecord == null) {
                shellDescriptorRecord = new ShellDescriptorRecord { SerialNumber = 1 };
                _contentStorageManager.Store(shellDescriptorRecord);
            }
            else {
                shellDescriptorRecord.SerialNumber++;
            }

            shellDescriptorRecord.Features.Clear();
            foreach (var feature in enabledFeatures) {
                shellDescriptorRecord.Features.Add(new ShellFeatureRecord { Name = feature.Name, ShellDescriptorRecord = shellDescriptorRecord });
            }
            _logger.LogDebug("Enabled features for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", enabledFeatures.Select(feature => feature.Name)));


            shellDescriptorRecord.Parameters.Clear();
            foreach (var parameter in parameters) {
                shellDescriptorRecord.Parameters.Add(new ShellParameterRecord {
                    Component = parameter.Component,
                    Name = parameter.Name,
                    Value = parameter.Value,
                    ShellDescriptorRecord = shellDescriptorRecord
                });
            }
            _logger.LogDebug("Parameters for shell '{0}' set: {1}.", _shellSettings.Name, string.Join(", ", parameters.Select(parameter => parameter.Name + "-" + parameter.Value)));

            _logger.LogInformation("Shell descriptor updated for shell '{0}'.", _shellSettings.Name);

            //_eventNotifier.Notify<IShellDescriptorManagerEventHandler>(
            //    e => e.Changed(GetShellDescriptorFromRecord(shellDescriptorRecord), _shellSettings.Name));
        }
    }
}
