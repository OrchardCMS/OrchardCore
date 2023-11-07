using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class SelectUsersInRoleTaskDisplayDriver : ActivityDisplayDriver<SelectUsersInRoleTask, SelectUsersInRoleTaskViewModel>
    {
        protected override void EditActivity(SelectUsersInRoleTask activity, SelectUsersInRoleTaskViewModel model)
        {
            model.PropertyName = activity.PropertyName.Expression;
            model.RoleName = activity.RoleName.Expression;
        }

        protected override void UpdateActivity(SelectUsersInRoleTaskViewModel model, SelectUsersInRoleTask activity)
        {
            activity.PropertyName = new WorkflowExpression<string>(model.PropertyName);
            activity.RoleName = new WorkflowExpression<string>(model.RoleName);
        }
    }
}
