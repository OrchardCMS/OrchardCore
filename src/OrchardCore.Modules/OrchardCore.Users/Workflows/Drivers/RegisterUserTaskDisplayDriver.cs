using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class RegisterUserTaskDisplayDriver : ActivityDisplayDriver<RegisterUserTask, RegisterUserTaskViewModel>
    {
        protected override void EditActivity(RegisterUserTask activity, RegisterUserTaskViewModel model)
        {
            model.SendConfirmationEmail = activity.SendConfirmationEmail;
            model.ConfirmationEmailSubject = activity.ConfirmationEmailSubject.Expression;
            model.ConfirmationEmailTemplate = activity.ConfirmationEmailTemplate.Expression;
            model.RequireModeration = activity.RequireModeration;
        }

        protected override void UpdateActivity(RegisterUserTaskViewModel model, RegisterUserTask activity)
        {
            activity.SendConfirmationEmail = model.SendConfirmationEmail;
            activity.ConfirmationEmailSubject = new WorkflowExpression<string>(model.ConfirmationEmailSubject);
            activity.ConfirmationEmailTemplate = new WorkflowExpression<string>(model.ConfirmationEmailTemplate);
            activity.RequireModeration = model.RequireModeration;
        }
    }
}
