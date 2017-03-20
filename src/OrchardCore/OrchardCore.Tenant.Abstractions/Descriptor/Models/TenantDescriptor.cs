using System.Collections.Generic;

namespace OrchardCore.Tenant.Descriptor.Models
{
    /// <summary>
    /// Contains a snapshot of a tenant's enabled features.
    /// The information is drawn out of the tenant via ITenantDescriptorManager
    /// and is passed to the ICompositionStrategy to build the TenantBlueprint.
    /// </summary>
    public class TenantDescriptor
    {
        /// <summary>
        /// Gets or sets the version number of the tenant descriptor.
        /// </summary>
        public int SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the list of features in the tenant.
        /// </summary>
        public IList<TenantFeature> Features { get; set; } = new List<TenantFeature>();

        /// <summary>
        /// Gets or sets the list of parameters specific to this tenant.
        /// </summary>
        public IList<TenantParameter> Parameters { get; set; } = new List<TenantParameter>();
    }
}