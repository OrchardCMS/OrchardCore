using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.Models
{
    public class ActivityContext
    {
        public ActivityRecord ActivityRecord { get; set; }
        public IActivity Activity { get; set; }
    }
}
