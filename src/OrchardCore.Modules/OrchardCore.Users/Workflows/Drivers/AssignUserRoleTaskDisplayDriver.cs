using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class AssignUserRoleTaskDisplayDriver : ActivityDisplayDriver<AssignUserRoleTask, AssignUserRoleTaskViewModel>
    {
        protected override void EditActivity(AssignUserRoleTask activity, AssignUserRoleTaskViewModel model)
        {
            model.UserName = activity.UserName.Expression;
            model.RoleName = activity.RoleName.Expression;
        }

        protected override void UpdateActivity(AssignUserRoleTaskViewModel model, AssignUserRoleTask activity)
        {
            activity.UserName = new WorkflowExpression<string>(model.UserName);
            activity.RoleName = new WorkflowExpression<string>(model.RoleName);
        }
    }
}
