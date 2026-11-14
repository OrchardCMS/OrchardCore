using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers;

public sealed class CreateTenantTaskDisplayDriver : TenantTaskDisplayDriver<CreateTenantTask, CreateTenantTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public CreateTenantTaskDisplayDriver(IStringLocalizer<CreateTenantTaskDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateTenantTask activity, BuildEditorContext context)
    {
        return Combine(
            Initialize<CreateTenantTaskViewModel>("CreateTenantTask_Fields_Edit", model => EditActivityAsync(activity, model))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<CreateTenantTaskViewModel>("CreateTenantTaskUrl_Fields_Edit", model => EditActivityAsync(activity, model))
                .Location(l => l.Zone("Content", "1").Card(S["URL"], "20")),
            Initialize<CreateTenantTaskViewModel>("CreateTenantTaskSetup_Fields_Edit", model => EditActivityAsync(activity, model))
                .Location(l => l.Zone("Content", "1").Card(S["Setup presets"], "30")),
            Initialize<CreateTenantTaskViewModel>("CreateTenantTaskFeatures_Fields_Edit", model => EditActivityAsync(activity, model))
                .Location(l => l.Zone("Content", "1").Card(S["Features"], "40")));
    }

    protected override void EditActivity(CreateTenantTask activity, CreateTenantTaskViewModel model)
    {
        model.TenantNameExpression = activity.TenantName.Expression;
        model.DescriptionExpression = activity.Description.Expression;
        model.RequestUrlPrefixExpression = activity.RequestUrlPrefix.Expression;
        model.RequestUrlHostExpression = activity.RequestUrlHost.Expression;
        model.DatabaseProviderExpression = activity.DatabaseProvider.Expression;
        model.ConnectionStringExpression = activity.ConnectionString.Expression;
        model.TablePrefixExpression = activity.TablePrefix.Expression;
        model.SchemaExpression = activity.Schema.Expression;
        model.RecipeNameExpression = activity.RecipeName.Expression;
        model.FeatureProfileExpression = activity.FeatureProfile.Expression;
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
        activity.Schema = new WorkflowExpression<string>(model.SchemaExpression);
        activity.RecipeName = new WorkflowExpression<string>(model.RecipeNameExpression);
        activity.FeatureProfile = new WorkflowExpression<string>(model.FeatureProfileExpression);
    }
}
