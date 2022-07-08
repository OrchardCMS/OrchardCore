using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ForLoopTaskDisplayDriver : ActivityDisplayDriver<ForLoopTask, ForLoopTaskViewModel>
    {
        protected override void EditActivity(ForLoopTask activity, ForLoopTaskViewModel model)
        {
            model.FromExpression = activity.From.Expression;
            model.ToExpression = activity.To.Expression;
            model.LoopVariableName = activity.LoopVariableName;
            model.StepExpression = activity.Step.Expression;
        }

        protected override void UpdateActivity(ForLoopTaskViewModel model, ForLoopTask activity)
        {
            activity.From = new WorkflowExpression<double>(model.FromExpression);
            activity.To = new WorkflowExpression<double>(model.ToExpression);
            activity.Step = new WorkflowExpression<double>(model.StepExpression);
            activity.LoopVariableName = model.LoopVariableName;
        }
    }
}
