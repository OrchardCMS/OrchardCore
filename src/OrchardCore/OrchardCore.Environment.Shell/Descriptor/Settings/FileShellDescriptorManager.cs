using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Descriptor.Settings
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by returning the features from a configuration file.
    /// </summary>
    public class FileShellDescriptorManager : IShellDescriptorManager
    {
        private readonly ShellSettingsWithTenants _shellSettings;
        private readonly string _applicationFeatureId;
        private ShellDescriptor _shellDescriptor;

        public FileShellDescriptorManager(ShellSettingsWithTenants shellSettings, IHostingEnvironment hostingEnvironment)
        {
            _shellSettings = shellSettings ?? throw new ArgumentException(nameof(shellSettings));
            _applicationFeatureId = hostingEnvironment.ApplicationName;
        }

        public Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            if (_shellDescriptor == null)
            {
                var features = _shellSettings.Features.Select(x => new ShellFeature(x)).ToList();
                features.Insert(0, new ShellFeature(_applicationFeatureId));

                _shellDescriptor = new ShellDescriptor
                {
                    Features = features
                };
            }

            return Task.FromResult(_shellDescriptor);
        }

        public Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            return Task.CompletedTask;
        }
    }
}
