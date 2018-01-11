using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class NotifyTaskDisplay : ActivityDisplayDriver<NotifyTask, NotifyTaskViewModel>
    {
        protected override void Map(NotifyTask source, NotifyTaskViewModel target)
        {
            target.NotificationType = source.NotificationType;
            target.Message = source.Message.Expression;
        }

        protected override void Map(NotifyTaskViewModel source, NotifyTask target)
        {
            target.NotificationType = source.NotificationType;
            target.Message = new WorkflowExpression<string>(source.Message);
        }
    }
}
