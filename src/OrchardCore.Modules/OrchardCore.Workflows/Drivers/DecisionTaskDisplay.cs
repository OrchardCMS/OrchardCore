using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class DecisionTaskDisplay : ActivityDisplayDriver<DecisionTask, DecisionTaskViewModel>
    {
        protected override void Map(DecisionTask source, DecisionTaskViewModel target)
        {
            target.ConditionExpression = source.ConditionExpression.Expression;
        }

        protected override void Map(DecisionTaskViewModel source, DecisionTask target)
        {
            target.ConditionExpression = new WorkflowExpression<bool>(source.ConditionExpression);
        }
    }
}
