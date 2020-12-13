using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class SignalEventDisplayDriver : ActivityDisplayDriver<SignalEvent, SignalEventViewModel>
    {
        protected override void EditActivity(SignalEvent activity, SignalEventViewModel model)
        {
            model.SignalNameExpression = activity.SignalName.Expression;
        }

        protected override void UpdateActivity(SignalEventViewModel model, SignalEvent activity)
        {
            activity.SignalName = new WorkflowExpression<string>(model.SignalNameExpression);
        }
    }
}
