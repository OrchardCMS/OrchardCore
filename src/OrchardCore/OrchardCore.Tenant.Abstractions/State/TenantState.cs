using System.Collections.Generic;

namespace OrchardCore.Tenant.State
{
    /// <summary>
    /// Represents the transitive list of features a tenant is made of at a specific moment.
    /// It's used to differentiate new features from existing ones in order to trigger events like
    /// Installed/Unistalled compared to only Enabled/Disabled.
    /// </summary>
    public class TenantState
    {
        public List<TenantFeatureState> Features { get; } = new List<TenantFeatureState>();
    }
}
