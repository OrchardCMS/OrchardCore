using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;

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

        private ShellDescriptor GetCurrentShell() {
            return _shellDescriptorManager.GetShellDescriptorAsync().Result;
        }

        public IEnumerable<IFeatureInfo> EnabledFeatures()
        {
            return _extensionManager.EnabledFeatures(GetCurrentShell());
        }

        public IEnumerable<IFeatureInfo> EnableFeatures(IEnumerable<IFeatureInfo> features)
        {
            return _shellDescriptorFeaturesManager.EnableFeatures(GetCurrentShell(), features);
        }

        public IEnumerable<IFeatureInfo> EnableFeatures(IEnumerable<IFeatureInfo> features, bool force)
        {
            return _shellDescriptorFeaturesManager.EnableFeatures(GetCurrentShell(), features, force);
        }

        public IEnumerable<IFeatureInfo> DisabledFeatures()
        {
            return _extensionManager.DisabledFeatures(GetCurrentShell());
        }

        public IEnumerable<IFeatureInfo> DisableFeatures(IEnumerable<IFeatureInfo> features)
        {
            return _shellDescriptorFeaturesManager.DisableFeatures(GetCurrentShell(), features);
        }

        public IEnumerable<IFeatureInfo> DisableFeatures(IEnumerable<IFeatureInfo> features, bool force)
        {
            return _shellDescriptorFeaturesManager.DisableFeatures(GetCurrentShell(), features, force);
        }

        public IEnumerable<string> GetDependentFeatures(string featureId)
        {
            return _shellDescriptorFeaturesManager.GetDependentFeatures(GetCurrentShell(), featureId);
        }
    }
}
