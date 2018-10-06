using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class EnableTenantTaskDisplay : TenantTaskDisplayDriver<EnableTenantTask, EnableTenantTaskViewModel>
    {
        protected override void EditActivity(EnableTenantTask activity, EnableTenantTaskViewModel model)
        {
            model.TenantName = activity.TenantName;
        }

        protected override void UpdateActivity(EnableTenantTaskViewModel model, EnableTenantTask activity)
        {
            activity.TenantName = model.TenantName;
        }
    }
}
