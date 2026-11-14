using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers;

public sealed class EnableTenantTaskDisplayDriver : TenantTaskDisplayDriver<EnableTenantTask, EnableTenantTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public EnableTenantTaskDisplayDriver(IStringLocalizer<EnableTenantTaskDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EnableTenantTask activity, BuildEditorContext context)
    {
        return Initialize<EnableTenantTaskViewModel>("EnableTenantTask_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Tenant"], "10"));
    }

    protected override void EditActivity(EnableTenantTask activity, EnableTenantTaskViewModel model)
    {
        model.TenantNameExpression = activity.TenantName.Expression;
    }

    protected override void UpdateActivity(EnableTenantTaskViewModel model, EnableTenantTask activity)
    {
        activity.TenantName = new WorkflowExpression<string>(model.TenantNameExpression);
    }
}
