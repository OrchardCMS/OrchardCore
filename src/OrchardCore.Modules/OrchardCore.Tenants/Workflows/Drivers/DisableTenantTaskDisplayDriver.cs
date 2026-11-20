using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tenants.Workflows.Drivers;

public sealed class DisableTenantTaskDisplayDriver : TenantTaskDisplayDriver<DisableTenantTask, DisableTenantTaskViewModel>
{
    internal readonly IStringLocalizer S;

    public DisableTenantTaskDisplayDriver(IStringLocalizer<DisableTenantTaskDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(DisableTenantTask activity, BuildEditorContext context)
    {
        return Initialize<DisableTenantTaskViewModel>("DisableTenantTask_Fields_Edit", model => EditActivityAsync(activity, model))
            .Location(l => l.Zone("Content", "1").Card(S["Tenant"], "10"));
    }

    protected override void EditActivity(DisableTenantTask activity, DisableTenantTaskViewModel model)
    {
        model.TenantNameExpression = activity.TenantName.Expression;
    }

    protected override void UpdateActivity(DisableTenantTaskViewModel model, DisableTenantTask activity)
    {
        activity.TenantName = new WorkflowExpression<string>(model.TenantNameExpression);
    }
}
