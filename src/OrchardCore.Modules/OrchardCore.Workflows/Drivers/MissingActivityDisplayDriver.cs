using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Workflows.Drivers;

public sealed class MissingActivityDisplayDriver : ActivityDisplayDriver<MissingActivity>
{
    public override IDisplayResult Display(MissingActivity activity, BuildDisplayContext context)
    {
        return View($"MissingActivity_Fields_Design", activity)
            .Location("Design", "Content");
    }
}
