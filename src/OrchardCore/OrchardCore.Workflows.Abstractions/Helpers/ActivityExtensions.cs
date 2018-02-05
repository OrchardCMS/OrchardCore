using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.Helpers
{
    public static class ActivityExtensions
    {
        public static bool IsEvent(this IActivity activity)
        {
            return activity is IEvent;
        }
    }
}
