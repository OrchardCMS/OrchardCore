using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell.Descriptor.Settings
{
    /// <summary>
    /// Implements <see cref="IShellDescriptorManager"/> by returning a single tenant with a specified set
    /// of features. This class can be registered as a singleton as its state never changes.
    /// </summary>
    public class SetFeaturesShellDescriptorManager : IShellDescriptorManager
    {
        private readonly IEnumerable<ShellFeature> _shellFeatures;
        private readonly IExtensionManager _extensionManager;

        private ShellDescriptor _shellDescriptor;

        public SetFeaturesShellDescriptorManager(IEnumerable<ShellFeature> shellFeatures, IExtensionManager extensionManager)
        {
            _shellFeatures = shellFeatures;
            _extensionManager = extensionManager;
        }

        public async Task<ShellDescriptor> GetShellDescriptorAsync()
        {
            if (_shellDescriptor == null)
            {
                var featureIds = _shellFeatures.Distinct().Select(sf => sf.Id).ToArray();

                var missingDependencies = (await _extensionManager.LoadFeaturesAsync(featureIds))
                    .Select(entry => entry.FeatureInfo.Id)
                    .Except(featureIds)
                    .Select(id => new ShellFeature(id));

                _shellDescriptor = new ShellDescriptor
                {
                    Features = _shellFeatures
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
    }
}
