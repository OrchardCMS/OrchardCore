using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class ValidateUserTaskDisplayDriver : ActivityDisplayDriver<ValidateUserTask, ValidateUserTaskViewModel>
    {
        protected override void EditActivity(ValidateUserTask activity, ValidateUserTaskViewModel model)
        {
            model.Roles = activity.Roles;
            model.SetUserName = activity.SetUserName;
        }

        protected override void UpdateActivity(ValidateUserTaskViewModel model, ValidateUserTask activity)
        {
            activity.Roles = model.Roles;
            activity.SetUserName = model.SetUserName;
        }
    }
}
