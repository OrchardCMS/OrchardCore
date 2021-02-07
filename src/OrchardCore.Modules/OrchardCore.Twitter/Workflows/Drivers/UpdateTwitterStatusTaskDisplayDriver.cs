using OrchardCore.Twitter.Workflows.Activities;
using OrchardCore.Twitter.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Twitter.Workflows.Drivers
{
    public class UpdateTwitterStatusTaskDisplayDriver : ActivityDisplayDriver<UpdateTwitterStatusTask, UpdateTwitterStatusTaskViewModel>
    {
        protected override void EditActivity(UpdateTwitterStatusTask activity, UpdateTwitterStatusTaskViewModel model)
        {
            model.StatusTemplate = activity.StatusTemplate.Expression;
        }

        protected override void UpdateActivity(UpdateTwitterStatusTaskViewModel model, UpdateTwitterStatusTask activity)
        {
            activity.StatusTemplate = new WorkflowExpression<string>(model.StatusTemplate);
        }
    }
}
