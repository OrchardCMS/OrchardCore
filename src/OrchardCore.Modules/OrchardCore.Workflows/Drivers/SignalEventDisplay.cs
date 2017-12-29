using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class SignalEventDisplay : ActivityDisplayDriver<SignalEvent, SignalEventViewModel>
    {
        protected override void Map(SignalEvent source, SignalEventViewModel target)
        {
            target.SignalName = source.SignalName;
        }

        protected override void Map(SignalEventViewModel source, SignalEvent target)
        {
            target.SignalName = source.SignalName;
        }
    }
}
