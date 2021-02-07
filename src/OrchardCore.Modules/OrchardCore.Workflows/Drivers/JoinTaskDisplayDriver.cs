using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class JoinTaskDisplayDriver : ActivityDisplayDriver<JoinTask, JoinTaskViewModel>
    {
        protected override void EditActivity(JoinTask activity, JoinTaskViewModel model)
        {
            model.Mode = activity.Mode;
        }

        protected override void UpdateActivity(JoinTaskViewModel model, JoinTask activity)
        {
            activity.Mode = model.Mode;
        }
    }
}
