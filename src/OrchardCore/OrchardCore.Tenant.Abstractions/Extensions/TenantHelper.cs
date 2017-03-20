using OrchardCore.Tenant.Models;

namespace OrchardCore.Tenant
{
    public static class TenantHelper
    {
        public const string DefaultTenantName = "Default";

        public static TenantSettings BuildDefaultUninitializedTenant = new TenantSettings {
            Name = DefaultTenantName,
            State = TenantState.Uninitialized };
    }
}