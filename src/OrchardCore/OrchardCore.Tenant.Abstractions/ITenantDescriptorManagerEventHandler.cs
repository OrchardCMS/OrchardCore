using OrchardCore.Tenant.Descriptor.Models;
using Orchard.Events;
using System.Threading.Tasks;

namespace OrchardCore.Tenant
{
    /// <summary>
    /// Represent and event handler for tenant descriptor.
    /// </summary>
    public interface ITenantDescriptorManagerEventHandler : IEventHandler
    {
        /// <summary>
        /// Triggered whenever a tenant descriptor has changed.
        /// </summary>
        Task Changed(TenantDescriptor descriptor, string tenant);
    }
}
