using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Extensions;
using OrchardCore.Tenant.Descriptor.Models;

namespace OrchardCore.Tenant.Descriptor.Settings
{
    /// <summary>
    /// Implements <see cref="ITenantDescriptorManager"/> by returning all pre defined list of features.
    /// </summary>
    public class SetFeaturesTenantDescriptorManager : ITenantDescriptorManager
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<TenantFeature> _tenantFeatures;
        private TenantDescriptor _tenantDescriptor;

        public SetFeaturesTenantDescriptorManager(IExtensionManager extensionManager,
            IEnumerable<TenantFeature> tenantFeatures)
        {
            _extensionManager = extensionManager;
            _tenantFeatures = tenantFeatures;
        }

        public Task<TenantDescriptor> GetTenantDescriptorAsync()
        {
            if (_tenantDescriptor == null)
            {
                _tenantDescriptor = new TenantDescriptor
                {
                    Features = _tenantFeatures.ToList()
                };
            }

            return Task.FromResult(_tenantDescriptor);
        }

        public Task UpdateTenantDescriptorAsync(int priorSerialNumber, IEnumerable<TenantFeature> enabledFeatures, IEnumerable<TenantParameter> parameters)
        {
            return Task.CompletedTask;
        }
    }
}
