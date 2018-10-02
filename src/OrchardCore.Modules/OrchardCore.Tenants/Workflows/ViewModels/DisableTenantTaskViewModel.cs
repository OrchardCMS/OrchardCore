using OrchardCore.Tenants.Workflows.Activities;

namespace OrchardCore.Tenants.Workflows.ViewModels
{
    public class DisableTenantTaskViewModel : TenantTaskViewModel<DisableTenantTask>
    {
        /// <summary>
        /// The expression resulting into a tenant to disable.
        /// </summary>
        public string Expression { get; set; }
    }
}
