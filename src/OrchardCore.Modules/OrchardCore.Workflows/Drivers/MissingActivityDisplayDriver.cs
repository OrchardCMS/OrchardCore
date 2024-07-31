using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Workflows.Drivers
{
    public class MissingActivityDisplayDriver : ActivityDisplayDriver<MissingActivity>
    {
        public override Task<IDisplayResult> DisplayAsync(MissingActivity activity, BuildDisplayContext context)
        {
            return Task.FromResult<IDisplayResult>(
                View($"MissingActivity_Fields_Design", activity)
                .Location("Design", "Content")
            );
        }
    }
}
