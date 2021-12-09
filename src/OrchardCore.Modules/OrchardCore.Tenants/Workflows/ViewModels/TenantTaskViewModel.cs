using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Tenants.Workflows.ViewModels
{
    public class TenantTaskViewModel<T> : ActivityViewModel<T> where T : TenantTask
    {
        public string TenantNameExpression { get; set; }

        public TenantTaskViewModel()
        {
        }

        public TenantTaskViewModel(T activity)
        {
            Activity = activity;
        }
    }
}
