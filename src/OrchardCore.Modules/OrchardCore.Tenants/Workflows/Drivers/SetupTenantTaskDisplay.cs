using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class SetupTenantTaskDisplay : TenantTaskDisplayDriver<SetupTenantTask, SetupTenantTaskViewModel>
    {
        private readonly IShellSettingsManager _shellSettingsManager;

        public SetupTenantTaskDisplay(IShellSettingsManager shellSettingsManager)
        {
            _shellSettingsManager = shellSettingsManager;
        }

        protected override void EditActivity(SetupTenantTask activity, SetupTenantTaskViewModel model)
        {
            model.TenantName = activity.TenantName;
            model.SiteNameExpression = activity.SiteName.Expression;
            model.AdminUsernameExpression = activity.AdminUsername.Expression;
            model.AdminEmailExpression= activity.AdminEmail.Expression;
            model.AdminPasswordExpression = activity.AdminPassword.Expression;
            model.DatabaseProviderExpression = activity.DatabaseProvider.Expression;
            model.DatabaseConnectionStringExpression = activity.DatabaseConnectionString.Expression;
            model.DatabaseTablePrefixExpression = activity.DatabaseTablePrefix.Expression;
            model.RecipeName = activity.RecipeName.Expression;
        }

        protected override void UpdateActivity(SetupTenantTaskViewModel model, SetupTenantTask activity)
        {
            activity.TenantName = model.TenantName;
            activity.SiteName = new WorkflowExpression<string>(model.SiteNameExpression);
            activity.AdminUsername = new WorkflowExpression<string>(model.AdminUsernameExpression);
            activity.AdminEmail = new WorkflowExpression<string>(model.AdminEmailExpression);
            activity.AdminPassword = new WorkflowExpression<string>(model.AdminPasswordExpression);
            activity.DatabaseProvider = new WorkflowExpression<string>(model.DatabaseProviderExpression);
            activity.DatabaseConnectionString = new WorkflowExpression<string>(model.DatabaseConnectionStringExpression);
            activity.DatabaseTablePrefix = new WorkflowExpression<string>(model.DatabaseTablePrefixExpression);
            activity.RecipeName = new WorkflowExpression<string>(model.RecipeName);
        }
    }
}
