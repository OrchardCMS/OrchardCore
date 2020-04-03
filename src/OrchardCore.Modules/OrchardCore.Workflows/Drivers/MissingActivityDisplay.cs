using OrchardCore.Workflows.Display;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.Drivers
{
    public class MissingActivityDisplay : ActivityDisplayDriver<MissingActivity>
    {
        public override IDisplayResult Display(MissingActivity activity)
        {
            return View($"MissingActivity_Fields_Design", activity).Location("Design", "Content");
        }
    }
}
