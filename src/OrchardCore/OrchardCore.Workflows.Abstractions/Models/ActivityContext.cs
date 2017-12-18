using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models
{
    public class ActivityContext
    {
        public IActivity Activity { get; set; }
        public ActivityRecord Record { get; set; }
    }
}