using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Orchard.Environment.Shell.Descriptor.Settings
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by returning all pre defined list of features.
    /// </summary>
    public class SetFeaturesShellDescriptorManager : IShellDescriptorManager
    {
        private readonly ShellSettings _shellSettings;
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<ShellFeature> _shellFeatures;
        private ShellDescriptor _shellDescriptor;

        public SetFeaturesShellDescriptorManager(
            ShellSettings shellSettings,
            IExtensionManager extensionManager,
            IEnumerable<ShellFeature> shellFeatures)
        {
            _shellSettings = shellSettings;
            _extensionManager = extensionManager;
            _shellFeatures = shellFeatures;
        }

        public Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            if (_shellDescriptor == null)
            {
                _shellDescriptor = new ShellDescriptor
                {
                    Features = _shellFeatures.ToList()
                };

                for (int i = 0; i < _shellSettings.Features.Length; i++)
                {
                    _shellDescriptor.Features.Add(new ShellFeature(_shellSettings.Features[i]));
                }
            }

            return Task.FromResult(_shellDescriptor);
        }

        public Task UpdateShellDescriptorAsync(int priorSerialNumber, IEnumerable<ShellFeature> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            return Task.CompletedTask;
        }
    }
}
