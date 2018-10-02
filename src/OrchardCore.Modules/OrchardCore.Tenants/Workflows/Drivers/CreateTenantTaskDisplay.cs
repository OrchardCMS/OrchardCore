using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class CreateTenantTaskDisplay : TenantTaskDisplayDriver<CreateTenantTask, CreateTenantTaskViewModel>
    {
        private readonly IShellSettingsManager _shellSettingsManager;

        public CreateTenantTaskDisplay(IShellSettingsManager shellSettingsManager)
        {
            _shellSettingsManager = shellSettingsManager;
        }

        protected override void EditActivity(CreateTenantTask activity, CreateTenantTaskViewModel model)
        {
            model.Name = activity.ContentType;
            model.TenantProperties = activity.TenantProperties.Expression;
        }

        protected override void UpdateActivity(CreateTenantTaskViewModel model, CreateTenantTask activity)
        {
            activity.ContentType = model.Name;
            activity.TenantProperties = new WorkflowExpression<string>(model.TenantProperties);
        }
    }
}
