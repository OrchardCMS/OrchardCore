using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class RegisterUserTaskDisplay : ActivityDisplayDriver<RegisterUserTask, RegisterUserTaskViewModel>
    {
        protected override void EditActivity(RegisterUserTask activity, RegisterUserTaskViewModel model)
        {
            model.ConfirmUser = activity.ConfirmUser;
            model.ConfirmationEmailTemplate = activity.ConfirmationEmailTemplate.Expression;
        }

        protected override void UpdateActivity(RegisterUserTaskViewModel model, RegisterUserTask activity)
        {
            activity.ConfirmUser = model.ConfirmUser;
            activity.ConfirmationEmailTemplate = new WorkflowExpression<string>(model.ConfirmationEmailTemplate);
        }
    }
}
