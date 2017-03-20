using OrchardCore.Tenant.Builders.Models;
using OrchardCore.Tenant.Descriptor.Models;
using System.Threading.Tasks;

namespace OrchardCore.Tenant.Builders
{
    /// <summary>
    /// Service at the host level to transform the cachable descriptor into the loadable blueprint.
    /// </summary>
    public interface ICompositionStrategy
    {
        /// <summary>
        /// Using information from the IExtensionManager, transforms and populates all of the
        /// blueprint model the tenant builders will need to correctly initialize a tenant IoC container.
        /// </summary>
        Task<TenantBlueprint> ComposeAsync(TenantSettings settings, TenantDescriptor descriptor);
    }
}