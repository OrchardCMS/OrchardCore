using OrchardCore.Twitter.Workflows.Activities;
using OrchardCore.Twitter.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Twitter.Workflows.Drivers;

public sealed class UpdateTwitterStatusTaskDisplayDriver : ActivityDisplayDriver<UpdateXTwitterStatusTask, UpdateTwitterStatusTaskViewModel>
{
    protected override void EditActivity(UpdateXTwitterStatusTask activity, UpdateTwitterStatusTaskViewModel model)
    {
        model.StatusTemplate = activity.StatusTemplate.Expression;
    }

    protected override void UpdateActivity(UpdateTwitterStatusTaskViewModel model, UpdateXTwitterStatusTask activity)
    {
        activity.StatusTemplate = new WorkflowExpression<string>(model.StatusTemplate);
    }
}
