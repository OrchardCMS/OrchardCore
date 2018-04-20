using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ForLoopTaskDisplay : ActivityDisplayDriver<ForLoopTask, ForLoopTaskViewModel>
    {
        protected override void EditActivity(ForLoopTask activity, ForLoopTaskViewModel model)
        {
            model.CountExpression = activity.Count.Expression;
        }

        protected override void UpdateActivity(ForLoopTaskViewModel model, ForLoopTask activity)
        {
            activity.Count = new WorkflowExpression<int>(model.CountExpression);
        }
    }
}
