using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class DisableTenantTaskDisplayDriver : TenantTaskDisplayDriver<DisableTenantTask, DisableTenantTaskViewModel>
    {
        protected override void EditActivity(DisableTenantTask activity, DisableTenantTaskViewModel model)
        {
            model.TenantNameExpression = activity.TenantName.Expression;
        }

        protected override void UpdateActivity(DisableTenantTaskViewModel model, DisableTenantTask activity)
        {
            activity.TenantName = new WorkflowExpression<string>(model.TenantNameExpression);
        }
    }
}
