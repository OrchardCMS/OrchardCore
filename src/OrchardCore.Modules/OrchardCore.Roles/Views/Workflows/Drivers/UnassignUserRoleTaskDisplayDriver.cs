using OrchardCore.Roles.Workflows.Activities;
using OrchardCore.Roles.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Roles.Workflows.Drivers;

public sealed class UnassignUserRoleTaskDisplayDriver : ActivityDisplayDriver<UnassignUserRoleTask, UnassignUserRoleTaskViewModel>
{
    protected override void EditActivity(UnassignUserRoleTask activity, UnassignUserRoleTaskViewModel model)
    {
        model.UserName = activity.UserName.Expression;
        model.Roles = activity.Roles;
    }

    protected override void UpdateActivity(UnassignUserRoleTaskViewModel model, UnassignUserRoleTask activity)
    {
        activity.UserName = new WorkflowExpression<string>(model.UserName);
        activity.Roles = model.Roles;
    }
}
