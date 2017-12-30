using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SignalEventDisplay : ActivityDisplayDriver<SignalEvent, SignalEventViewModel>
    {
        protected override void Map(SignalEvent source, SignalEventViewModel target)
        {
            target.SignalName = source.SignalName;
            target.ConditionExpression = source.ConditionExpression.Expression;
        }

        protected override void Map(SignalEventViewModel source, SignalEvent target)
        {
            target.SignalName = source.SignalName.Trim();
            target.ConditionExpression = new Models.WorkflowExpression<bool>(source.ConditionExpression);
        }
    }
}
