using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Orchard.Environment.Shell
{
    public class ShellFeaturesManager : IShellFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IShellDescriptorFeaturesManager _shellDescriptorFeaturesManager;

        public ShellFeaturesManager(
            IExtensionManager extensionManager,
            ShellDescriptor shellDescriptor,
            IShellDescriptorFeaturesManager shellDescriptorFeaturesManager)
        {
            _extensionManager = extensionManager;
            _shellDescriptor = shellDescriptor;
            _shellDescriptorFeaturesManager = shellDescriptorFeaturesManager;
        }

        public Task<IEnumerable<IFeatureInfo>> GetEnabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => _shellDescriptor.Features.Any(sf => sf.Id == f.Id)));
        }

        public Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(IEnumerable<IFeatureInfo> features)
        {
            return _shellDescriptorFeaturesManager.EnableFeaturesAsync(_shellDescriptor, features);
        }

        public Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(IEnumerable<IFeatureInfo> features, bool force)
        {
            return _shellDescriptorFeaturesManager.EnableFeaturesAsync(_shellDescriptor, features, force);
        }

        public Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => _shellDescriptor.Features.All(sf => sf.Id != f.Id)));
        }

        public Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(IEnumerable<IFeatureInfo> features)
        {
            return _shellDescriptorFeaturesManager.DisableFeaturesAsync(_shellDescriptor, features);
        }

        public Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(IEnumerable<IFeatureInfo> features, bool force)
        {
            return _shellDescriptorFeaturesManager.DisableFeaturesAsync(_shellDescriptor, features, force);
        }
    }
}
