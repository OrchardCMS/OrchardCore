using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class DisableTenantTaskDisplay : TenantTaskDisplayDriver<DisableTenantTask, DisableTenantTaskViewModel>
    {
        protected override void EditActivity(DisableTenantTask activity, DisableTenantTaskViewModel model)
        {
            model.Expression = activity.Tenant.Expression;
        }

        protected override void UpdateActivity(DisableTenantTaskViewModel model, DisableTenantTask activity)
        {
            activity.Tenant = new WorkflowExpression<ShellSettings>(model.Expression);
        }
    }
}
