using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Extensions;
using OrchardCore.Tenant.Descriptor.Models;

namespace OrchardCore.Tenant.Descriptor.Settings
{
    /// <summary>
    /// Implements <see cref="ITenantDescriptorManager"/> by returning all the available features.
    /// </summary>
    public class AllFeaturesTenantDescriptorManager : ITenantDescriptorManager
    {
        private readonly IExtensionManager _extensionManager;
        private TenantDescriptor _tenantDescriptor;

        public AllFeaturesTenantDescriptorManager(IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager;
        }

        public Task<TenantDescriptor> GetTenantDescriptorAsync()
        {
            if (_tenantDescriptor == null)
            {
                _tenantDescriptor = new TenantDescriptor
                {
                    Features = _extensionManager.GetFeatures().Select(x => new TenantFeature { Id = x.Id }).ToList()
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