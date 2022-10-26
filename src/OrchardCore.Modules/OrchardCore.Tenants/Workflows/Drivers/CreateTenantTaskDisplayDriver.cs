using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class CreateTenantTaskDisplayDriver : TenantTaskDisplayDriver<CreateTenantTask, CreateTenantTaskViewModel>
    {
        protected override void EditActivity(CreateTenantTask activity, CreateTenantTaskViewModel model)
        {
            model.TenantNameExpression = activity.TenantName.Expression;
            model.DescriptionExpression = activity.Description.Expression;
            model.RequestUrlPrefixExpression = activity.RequestUrlPrefix.Expression;
            model.RequestUrlHostExpression = activity.RequestUrlHost.Expression;
            model.DatabaseProviderExpression = activity.DatabaseProvider.Expression;
            model.ConnectionStringExpression = activity.ConnectionString.Expression;
            model.TablePrefixExpression = activity.TablePrefix.Expression;
            model.RecipeNameExpression = activity.RecipeName.Expression;
            model.FeatureProfileExpression = activity.FeatureProfile.Expression;
            model.Schema = activity.Schema;
            model.DocumentTable = activity.DocumentTable;
            model.TableNameSeparator = activity.TableNameSeparator;
            model.IdentityColumnSize = activity.IdentityColumnSize;
        }

        protected override void UpdateActivity(CreateTenantTaskViewModel model, CreateTenantTask activity)
        {
            activity.TenantName = new WorkflowExpression<string>(model.DescriptionExpression);
            activity.Description = new WorkflowExpression<string>(model.TenantNameExpression);
            activity.RequestUrlPrefix = new WorkflowExpression<string>(model.RequestUrlPrefixExpression);
            activity.RequestUrlHost = new WorkflowExpression<string>(model.RequestUrlHostExpression);
            activity.DatabaseProvider = new WorkflowExpression<string>(model.DatabaseProviderExpression);
            activity.ConnectionString = new WorkflowExpression<string>(model.ConnectionStringExpression);
            activity.TablePrefix = new WorkflowExpression<string>(model.TablePrefixExpression);
            activity.RecipeName = new WorkflowExpression<string>(model.RecipeNameExpression);
            activity.FeatureProfile = new WorkflowExpression<string>(model.FeatureProfileExpression);
            activity.Schema = model.Schema;
            activity.DocumentTable = model.DocumentTable;
            activity.TableNameSeparator = model.TableNameSeparator;
            activity.IdentityColumnSize = model.IdentityColumnSize;
        }
    }
}
