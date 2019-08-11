using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowTypePropertiesViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsSingleton { get; set; }
        public bool DeleteFinishedWorkflows { get; set; }
        public string ReturnUrl { get; set; }
    }
}