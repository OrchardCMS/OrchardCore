using System;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ForkTaskDisplay : ActivityDisplayDriver<ForkTask, ForkTaskViewModel>
    {
        protected override void Map(ForkTask source, ForkTaskViewModel target)
        {
            target.Forks = string.Join(", ", source.Forks);
        }

        protected override void Map(ForkTaskViewModel source, ForkTask target)
        {
            target.Forks = source.Forks.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
