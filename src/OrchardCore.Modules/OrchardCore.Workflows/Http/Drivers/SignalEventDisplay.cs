using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class SignalEventDisplay : ActivityDisplayDriver<SignalEvent, SignalEventViewModel>
    {
        protected override void Map(SignalEvent source, SignalEventViewModel target)
        {
            target.SignalNameExpression = source.SignalName.Expression;
        }

        protected override void Map(SignalEventViewModel source, SignalEvent target)
        {
            target.SignalName = new WorkflowExpression<string>(source.SignalNameExpression);
        }
    }
}
