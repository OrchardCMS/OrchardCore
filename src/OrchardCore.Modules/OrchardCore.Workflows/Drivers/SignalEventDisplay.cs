using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SignalEventDisplay : ActivityDisplayDriver<SignalEvent, SignalEventViewModel>
    {
        protected override void Map(SignalEvent source, SignalEventViewModel target)
        {
            target.SignalNameExpression = source.SignalNameExpression.Expression;
            target.ConditionExpression = source.ConditionExpression.Expression;
        }

        protected override void Map(SignalEventViewModel source, SignalEvent target)
        {
            target.SignalNameExpression = new WorkflowExpression<string>(source.SignalNameExpression);
            target.ConditionExpression = new WorkflowExpression<bool>(source.ConditionExpression);
        }
    }
}
