using OrchardCore.Roles.Workflows.Activities;
using OrchardCore.Roles.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Roles.Workflows.Drivers
{
    public class RemoveUserRoleTaskDisplayDriver : ActivityDisplayDriver<RemoveUserRoleTask, RemoveUserRoleTaskViewModel>
    {
        protected override void EditActivity(RemoveUserRoleTask activity, RemoveUserRoleTaskViewModel model)
        {
            model.UserName = activity.UserName.Expression;
            model.RoleName = activity.RoleName.Expression;
        }

        protected override void UpdateActivity(RemoveUserRoleTaskViewModel model, RemoveUserRoleTask activity)
        {
            activity.UserName = new WorkflowExpression<string>(model.UserName);
            activity.RoleName = new WorkflowExpression<string>(model.RoleName);
        }
    }
}
