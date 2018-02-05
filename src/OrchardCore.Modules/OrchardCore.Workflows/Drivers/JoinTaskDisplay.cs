using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class JoinTaskDisplay : ActivityDisplayDriver<JoinTask, JoinTaskViewModel>
    {
        protected override void Map(JoinTask source, JoinTaskViewModel target)
        {
            target.Mode = source.Mode;
        }

        protected override void Map(JoinTaskViewModel source, JoinTask target)
        {
            target.Mode = source.Mode;
        }
    }
}
