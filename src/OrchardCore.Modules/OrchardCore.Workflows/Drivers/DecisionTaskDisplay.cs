using System;
using System.Linq;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class DecisionTaskDisplay : ActivityDisplayDriver<DecisionTask, DecisionTaskViewModel>
    {
        protected override void Map(DecisionTask source, DecisionTaskViewModel target)
        {
            target.AvailableOutcomes = string.Join(", ", source.AvailableOutcomes);
            target.Script = source.Script;
        }

        protected override void Map(DecisionTaskViewModel source, DecisionTask target)
        {
            target.AvailableOutcomes = source.AvailableOutcomes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            target.Script = source.Script;
        }
    }
}
