using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Events;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class WorkflowFaultEventDisplayDriver : ActivityDisplayDriver<WorkflowFaultEvent, WorkflowFaultViewModel>
    {
        protected override void EditActivity(WorkflowFaultEvent activity, WorkflowFaultViewModel model)
        {
            model.ErrorFilter = activity.ErrorFilter.Expression;
        }

        protected override void UpdateActivity(WorkflowFaultViewModel model, WorkflowFaultEvent activity)
        {
            activity.ErrorFilter = new WorkflowExpression<bool>(model.ErrorFilter);
        }
    }
}
