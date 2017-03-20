using Microsoft.Extensions.Options;

namespace OrchardCore.Tenant
{
    /// <summary>
    /// Sets up default options for <see cref="TenantOptions"/>.
    /// </summary>
    public class TenantOptionsSetup : ConfigureOptions<TenantOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TenantOptions"/>.
        /// </summary>
        public TenantOptionsSetup()
            : base(options => { })
        {
        }
    }
}