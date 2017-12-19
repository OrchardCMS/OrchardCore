using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Models
{
    public class ActivityContext
    {
        public ActivityRecord Record { get; set; }
        public IActivity Activity { get; set; }
    }
}