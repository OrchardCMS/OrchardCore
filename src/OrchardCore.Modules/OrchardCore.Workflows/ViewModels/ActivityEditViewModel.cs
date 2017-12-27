using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.ViewModels
{
    public class ActivityEditViewModel
    {
        public dynamic ActivityEditor { get; set; }
        public IActivity Activity { get; set; }
        public int? ActivityId { get; set; }
        public int WorkflowDefinitionId { get; set; }
    }
}
