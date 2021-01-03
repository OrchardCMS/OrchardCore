using System.ComponentModel.DataAnnotations;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class WorkflowTypePropertiesViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsSingleton { get; set; }
        public int? LockTimeout { get; set; } = WorkflowType.DefaultLockTimeout;
        public int? LockExpiration { get; set; } = WorkflowType.DefaultLockExpiration;
        public bool DeleteFinishedWorkflows { get; set; }
        public string ReturnUrl { get; set; }
    }
}
