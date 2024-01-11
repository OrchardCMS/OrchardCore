namespace OrchardCore.Environment.Shell.Models
{
    /// <summary>
    /// The different states of a Tenant.
    /// </summary>
    public enum TenantState
    {
        /// <summary>
        /// The tenant is not yet initialized.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// The tenant is being initialized.
        /// </summary>
        Initializing,

        /// <summary>
        /// The tenant is initialized and running.
        /// </summary>
        Running,

        /// <summary>
        /// The tenant is initialized and disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// The tenant settings are invalid.
        /// </summary>
        Invalid
    }
}
