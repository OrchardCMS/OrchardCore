using OrchardCore.Tenants.Workflows.Activities;

namespace OrchardCore.Tenants.Workflows.ViewModels
{
    public class TenantSetupEventViewModel : TenantEventViewModel<TenantSetupEvent>
    {
        public TenantSetupEventViewModel()
        {
        }

        public TenantSetupEventViewModel(TenantSetupEvent activity)
        {
            Activity = activity;
        }
    }
}
