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
            model.TenantName = activity.TenantName;
            model.RequestUrlPrefixExpression = activity.RequestUrlPrefix.Expression;
            model.RequestUrlHostExpression = activity.RequestUrlHost.Expression;
            model.DatabaseProviderExpression = activity.DatabaseProvider.Expression;
            model.ConnectionStringExpression = activity.ConnectionString.Expression;
            model.TablePrefixExpression = activity.TablePrefix.Expression;
            model.RecipeName = activity.RecipeName.Expression;
        }

        protected override void UpdateActivity(CreateTenantTaskViewModel model, CreateTenantTask activity)
        {
            activity.TenantName = model.TenantName;
            activity.RequestUrlPrefix = new WorkflowExpression<string>(model.RequestUrlPrefixExpression);
            activity.RequestUrlHost = new WorkflowExpression<string>(model.RequestUrlHostExpression);
            activity.DatabaseProvider = new WorkflowExpression<string>(model.DatabaseProviderExpression);
            activity.ConnectionString = new WorkflowExpression<string>(model.ConnectionStringExpression);
            activity.TablePrefix = new WorkflowExpression<string>(model.TablePrefixExpression);
            activity.RecipeName = new WorkflowExpression<string>(model.RecipeName);
        }
    }
}
