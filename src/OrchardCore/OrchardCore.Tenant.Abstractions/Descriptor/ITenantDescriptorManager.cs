using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Tenant.Descriptor.Models;

namespace OrchardCore.Tenant.Descriptor
{
    /// <summary>
    /// Service resolved out of the tenant container. Primarily used by host.
    /// </summary>
    public interface ITenantDescriptorManager
    {
        /// <summary>
        /// Uses tenant-specific database or other resources to return
        /// the current "correct" configuration. The host will use this information
        /// to reinitialize the tenant.
        /// </summary>
        Task<TenantDescriptor> GetTenantDescriptorAsync();

        /// <summary>
        /// Alters databased information to match information passed as arguments.
        /// Prior SerialNumber used for optimistic concurrency, and an exception
        /// should be thrown if the number in storage doesn't match what's provided.
        /// </summary>
        Task UpdateTenantDescriptorAsync(
            int priorSerialNumber,
            IEnumerable<TenantFeature> enabledFeatures,
            IEnumerable<TenantParameter> parameters);
    }
}