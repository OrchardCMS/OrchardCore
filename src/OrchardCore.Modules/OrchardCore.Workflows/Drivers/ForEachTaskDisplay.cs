using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;
using System.Collections.Generic;

namespace OrchardCore.Workflows.Drivers
{
    public class ForEachTaskDisplay : ActivityDisplayDriver<ForEachTask, ForEachTaskViewModel>
    {
        protected override void EditActivity(ForEachTask activity, ForEachTaskViewModel model)
        {
            model.EnumerableExpression = activity.Enumerable.Expression;
        }

        protected override void UpdateActivity(ForEachTaskViewModel model, ForEachTask activity)
        {
            activity.Enumerable = new WorkflowExpression<IEnumerable<object>>(model.EnumerableExpression);
        }
    }
}
