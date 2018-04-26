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
            model.FromExpression = activity.From.Expression;
            model.ToExpression = activity.To.Expression;
            model.LoopVariableName = activity.LoopVariableName;
        }

        protected override void UpdateActivity(ForLoopTaskViewModel model, ForLoopTask activity)
        {
            activity.From = new WorkflowExpression<double>(model.FromExpression);
            activity.To = new WorkflowExpression<double>(model.ToExpression);
            activity.LoopVariableName = model.LoopVariableName;
        }
    }
}
