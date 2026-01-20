using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.ViewModels
{
    public class ActivityEditViewModel
    {
        public dynamic ActivityEditor { get; set; }
        public IActivity Activity { get; set; }
        public string ActivityId { get; set; }
        public long WorkflowTypeId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
