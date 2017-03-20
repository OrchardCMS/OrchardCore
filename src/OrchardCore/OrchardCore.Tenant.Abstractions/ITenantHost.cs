using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.Hosting.TenantBuilders;

namespace OrchardCore.Tenant
{
    public interface ITenantHost
    {
        /// <summary>
        /// Ensure that all the <see cref="TenantContext"/> are created and available to process requests.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Returns an existing <see cref="TenantContext"/> or creates a new one if necessary.
        /// </summary>
        /// <param name="settings">The <see cref="TenantSettings"/> object representing the tenant to get.</param>
        /// <returns></returns>
        TenantContext GetOrCreateTenantContext(TenantSettings settings);

        /// <summary>
        /// Updates an existing tenant configuration.
        /// </summary>
        /// <param name="settings"></param>
        void UpdateTenantSettings(TenantSettings settings);

        /// <summary>
        /// Reloads a tenant.
        /// </summary>
        /// <param name="settings"></param>
        void ReloadTenantContext(TenantSettings settings);

        /// <summary>
        /// Creates a new <see cref="TenantContext"/>.
        /// </summary>
        /// <param name="settings">The <see cref="TenantSettings"/> object representing the tenant to create.</param>
        /// <returns></returns>
        Task<TenantContext> CreateTenantContextAsync(TenantSettings settings);

        /// <summary>
        /// Lists all available <see cref="TenantContext"/> instances.
        /// </summary>
        /// <remarks>A tenant might not be listed if it hasn't been created yet, for instance if it has been removed and not yet recreated.</remarks>
        IEnumerable<TenantContext> ListTenantContexts();
    }
}