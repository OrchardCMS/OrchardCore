using OrchardCore.Roles.Workflows.Activities;
using OrchardCore.Roles.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Roles.Workflows.Drivers
{
    public class SelectUsersInRoleTaskDisplayDriver : ActivityDisplayDriver<SelectUsersInRoleTask, SelectUsersInRoleTaskViewModel>
    {
        protected override void EditActivity(SelectUsersInRoleTask activity, SelectUsersInRoleTaskViewModel model)
        {
            model.OutputKeyName = activity.OutputKeyName.Expression;
            model.Roles = activity.Roles;
        }

        protected override void UpdateActivity(SelectUsersInRoleTaskViewModel model, SelectUsersInRoleTask activity)
        {
            activity.OutputKeyName = new WorkflowExpression<string>(model.OutputKeyName);
            activity.Roles = model.Roles;
        }
    }
}
