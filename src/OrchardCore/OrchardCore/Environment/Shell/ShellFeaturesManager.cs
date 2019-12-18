using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Environment.Shell
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

        public Task<IEnumerable<IFeatureInfo>> GetAlwaysEnabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => f.IsAlwaysEnabled || _shellDescriptor.Features.Any(sf => sf.Id == f.Id && sf.AlwaysEnabled)));
        }

        public Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => _shellDescriptor.Features.All(sf => sf.Id != f.Id)));
        }

        public Task<(IEnumerable<IFeatureInfo>, IEnumerable<IFeatureInfo>)> UpdateFeaturesAsync(IEnumerable<IFeatureInfo> featuresToDisable, IEnumerable<IFeatureInfo> featuresToEnable, bool force)
        {
            return _shellDescriptorFeaturesManager.UpdateFeaturesAsync(_shellDescriptor, featuresToDisable, featuresToEnable, force);
        }

        public Task<IEnumerable<IExtensionInfo>> GetEnabledExtensionsAsync()
        {
            // enabled extensions are those which have at least one enabled feature.
            var enabledIds = _extensionManager.GetFeatures().Where(f => _shellDescriptor
                .Features.Any(sf => sf.Id == f.Id)).Select(f => f.Extension.Id).Distinct().ToArray();

            // Extensions are still ordered according to the weight of their first features.
            return Task.FromResult(_extensionManager.GetExtensions().Where(e => enabledIds.Contains(e.Id)));
        }
    }
}
