using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class EnableTenantTaskDisplayDriver : TenantTaskDisplayDriver<EnableTenantTask, EnableTenantTaskViewModel>
    {
        protected override void EditActivity(EnableTenantTask activity, EnableTenantTaskViewModel model)
        {
            model.TenantNameExpression = activity.TenantName.Expression;
        }

        protected override void UpdateActivity(EnableTenantTaskViewModel model, EnableTenantTask activity)
        {
            activity.TenantName = new WorkflowExpression<string>(model.TenantNameExpression);
        }
    }
}
