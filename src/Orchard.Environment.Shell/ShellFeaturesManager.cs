using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Environment.Shell
{
    public class ShellFeaturesManager : IShellFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IShellDescriptorFeaturesManager _shellDescriptorFeaturesManager;

        public ShellFeaturesManager(
            IExtensionManager extensionManager,
            IShellDescriptorManager shellDescriptorManager,
            IShellDescriptorFeaturesManager shellDescriptorFeaturesManager)
        {
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;
            _shellDescriptorFeaturesManager = shellDescriptorFeaturesManager;
        }

        private Task<ShellDescriptor> GetCurrentShell() {
            return _shellDescriptorManager.GetShellDescriptorAsync();
        }

        public async Task<IEnumerable<IFeatureInfo>> GetEnabledFeaturesAsync()
        {
            return _extensionManager.GetEnabledFeatures(await GetCurrentShell());
        }

        public async Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(IEnumerable<IFeatureInfo> features)
        {
            return await _shellDescriptorFeaturesManager.EnableFeaturesAsync(await GetCurrentShell(), features);
        }

        public async Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(IEnumerable<IFeatureInfo> features, bool force)
        {
            return await _shellDescriptorFeaturesManager.EnableFeaturesAsync(await GetCurrentShell(), features, force);
        }

        public async Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync()
        {
            return _extensionManager.GetDisabledFeatures(await GetCurrentShell());
        }

        public async Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(IEnumerable<IFeatureInfo> features)
        {
            return await _shellDescriptorFeaturesManager.DisableFeaturesAsync(await GetCurrentShell(), features);
        }

        public async Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(IEnumerable<IFeatureInfo> features, bool force)
        {
            return await _shellDescriptorFeaturesManager.DisableFeaturesAsync(await GetCurrentShell(), features, force);
        }

        public async Task<IEnumerable<string>> GetDependentFeaturesAsync(string featureId)
        {
            return await _shellDescriptorFeaturesManager.GetDependentFeaturesAsync(await GetCurrentShell(), featureId);
        }
    }
}
