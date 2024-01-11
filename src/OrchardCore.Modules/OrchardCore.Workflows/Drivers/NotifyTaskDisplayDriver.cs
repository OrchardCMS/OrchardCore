using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class NotifyTaskDisplayDriver : ActivityDisplayDriver<NotifyTask, NotifyTaskViewModel>
    {
        protected override void EditActivity(NotifyTask activity, NotifyTaskViewModel model)
        {
            model.NotificationType = activity.NotificationType;
            model.Message = activity.Message.Expression;
        }

        protected override void UpdateActivity(NotifyTaskViewModel model, NotifyTask activity)
        {
            activity.NotificationType = model.NotificationType;
            activity.Message = new WorkflowExpression<string>(model.Message);
        }
    }
}
