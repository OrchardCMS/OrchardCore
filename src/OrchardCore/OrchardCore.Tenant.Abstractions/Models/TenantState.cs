namespace OrchardCore.Tenant.Models
{
    public enum TenantState
    {
        Uninitialized,
        Initializing,
        Running,
        Disabled,
        Invalid
    }
}