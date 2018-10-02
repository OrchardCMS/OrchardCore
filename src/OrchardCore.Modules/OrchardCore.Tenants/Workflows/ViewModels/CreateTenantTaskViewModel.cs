using OrchardCore.Tenants.Workflows.Activities;

namespace OrchardCore.Tenants.Workflows.ViewModels
{
    public class CreateTenantTaskViewModel : TenantTaskViewModel<CreateTenantTask>
    {
        public string Name { get; set; }
        public string TenantProperties { get; set; }
    }
}
