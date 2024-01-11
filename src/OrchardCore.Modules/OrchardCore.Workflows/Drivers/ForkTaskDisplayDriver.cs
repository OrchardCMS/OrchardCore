using System;
using System.Linq;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class ForkTaskDisplayDriver : ActivityDisplayDriver<ForkTask, ForkTaskViewModel>
    {
        protected override void EditActivity(ForkTask activity, ForkTaskViewModel model)
        {
            model.Forks = string.Join(", ", activity.Forks);
        }

        protected override void UpdateActivity(ForkTaskViewModel model, ForkTask activity)
        {
            activity.Forks = model.Forks.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}
