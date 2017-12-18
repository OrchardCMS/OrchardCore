using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Abstractions.Display
{
    public abstract class ActivityDisplayDriver<T> : DisplayDriver<IActivity, T> where T : class, IActivity
    {
    }
}
