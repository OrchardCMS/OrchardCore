using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Extensions;
using OrchardCore.Extensions.Features;
using OrchardCore.Tenant.Descriptor.Models;

namespace OrchardCore.Tenant
{
    public class TenantFeaturesManager : ITenantFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly TenantDescriptor _tenantDescriptor;
        private readonly ITenantDescriptorFeaturesManager _tenantDescriptorFeaturesManager;

        public TenantFeaturesManager(
            IExtensionManager extensionManager,
            TenantDescriptor tenantDescriptor,
            ITenantDescriptorFeaturesManager tenantDescriptorFeaturesManager)
        {
            _extensionManager = extensionManager;
            _tenantDescriptor = tenantDescriptor;
            _tenantDescriptorFeaturesManager = tenantDescriptorFeaturesManager;
        }

        public Task<IEnumerable<IFeatureInfo>> GetEnabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => _tenantDescriptor.Features.Any(sf => sf.Id == f.Id)));
        }

        public Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(IEnumerable<IFeatureInfo> features)
        {
            return _tenantDescriptorFeaturesManager.EnableFeaturesAsync(_tenantDescriptor, features);
        }

        public Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(IEnumerable<IFeatureInfo> features, bool force)
        {
            return _tenantDescriptorFeaturesManager.EnableFeaturesAsync(_tenantDescriptor, features, force);
        }

        public Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync()
        {
            return Task.FromResult(_extensionManager.GetFeatures().Where(f => _tenantDescriptor.Features.All(sf => sf.Id != f.Id)));
        }

        public Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(IEnumerable<IFeatureInfo> features)
        {
            return _tenantDescriptorFeaturesManager.DisableFeaturesAsync(_tenantDescriptor, features);
        }

        public Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(IEnumerable<IFeatureInfo> features, bool force)
        {
            return _tenantDescriptorFeaturesManager.DisableFeaturesAsync(_tenantDescriptor, features, force);
        }
    }
}
