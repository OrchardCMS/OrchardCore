using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowFaultViewModel
    {
        [Required]
        public string ErrorFilter { get; set; }
    }
}
