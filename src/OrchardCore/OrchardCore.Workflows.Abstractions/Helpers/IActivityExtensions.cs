using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Helpers
{
    public static class IActivityExtensions
    {
        public static bool IsEvent(this IActivity activity)
        {
            return activity is IEvent;
        }
    }
}
