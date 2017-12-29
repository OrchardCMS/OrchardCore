using System;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class BranchTaskDisplay : ActivityDisplayDriver<BranchTask, BranchTaskViewModel>
    {
        protected override void Map(BranchTask source, BranchTaskViewModel target)
        {
            target.Branches = string.Join(", ", source.Branches);
        }

        protected override void Map(BranchTaskViewModel source, BranchTask target)
        {
            target.Branches = source.Branches.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
