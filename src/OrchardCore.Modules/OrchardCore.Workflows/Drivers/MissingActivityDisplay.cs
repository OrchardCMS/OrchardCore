using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;

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
