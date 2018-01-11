using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Workflows.Drivers
{
    public class MissingActivityDisplay : ActivityDisplayDriver<MissingActivity>
    {
        public override IDisplayResult Display(MissingActivity activity)
        {
            return Shape($"MissingActivity_Fields_Design", activity).Location("Design", "Content");
        }
    }
}
