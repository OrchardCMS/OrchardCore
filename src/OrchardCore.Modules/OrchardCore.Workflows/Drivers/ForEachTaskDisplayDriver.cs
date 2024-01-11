using System.Collections.Generic;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ForEachTaskDisplayDriver : ActivityDisplayDriver<ForEachTask, ForEachTaskViewModel>
    {
        protected override void EditActivity(ForEachTask activity, ForEachTaskViewModel model)
        {
            model.EnumerableExpression = activity.Enumerable.Expression;
            model.LoopVariableName = activity.LoopVariableName;
        }

        protected override void UpdateActivity(ForEachTaskViewModel model, ForEachTask activity)
        {
            activity.LoopVariableName = model.LoopVariableName;
            activity.Enumerable = new WorkflowExpression<IEnumerable<object>>(model.EnumerableExpression);
        }
    }
}
