using OrchardCore.Tenant.Descriptor.Models;
using Orchard.Hosting.TenantBuilders;
using System.Threading.Tasks;

namespace OrchardCore.Tenant.Builders
{
    /// <summary>
    /// High-level coordinator that exercises other component capabilities to
    /// build all of the artifacts for a running tenant given a tenant settings.
    /// </summary>
    public interface ITenantContextFactory
    {
        /// <summary>
        /// Builds a tenant context given a specific tenant settings structure
        /// </summary>
        Task<TenantContext> CreateTenantContextAsync(TenantSettings settings);

        /// <summary>
        /// Builds a tenant context for an uninitialized Orchard instance. Needed
        /// to display setup user interface.
        /// </summary>
        Task<TenantContext> CreateSetupContextAsync(TenantSettings settings);

        /// <summary>
        /// Builds a tenant context given a specific description of features and parameters.
        /// Tenant's actual current descriptor has no effect. Does not use or update descriptor cache.
        /// </summary>
        Task<TenantContext> CreateDescribedContextAsync(TenantSettings settings, TenantDescriptor tenantDescriptor);
    }
}