using OrchardCore.Roles.Workflows.Activities;
using OrchardCore.Roles.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Roles.Workflows.Drivers;

public sealed class GetUsersByRoleTaskDisplayDriver : ActivityDisplayDriver<GetUsersByRoleTask, GetUsersByRoleTaskViewModel>
{
    protected override void EditActivity(GetUsersByRoleTask activity, GetUsersByRoleTaskViewModel model)
    {
        model.OutputKeyName = activity.OutputKeyName.Expression;
        model.Roles = activity.Roles;
    }

    protected override void UpdateActivity(GetUsersByRoleTaskViewModel model, GetUsersByRoleTask activity)
    {
        activity.OutputKeyName = new WorkflowExpression<string>(model.OutputKeyName);
        activity.Roles = model.Roles;
    }
}
