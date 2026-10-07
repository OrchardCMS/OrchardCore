using System.ComponentModel.DataAnnotations;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class WorkflowTypePropertiesViewModel
{
    public long Id { get; set; }

    [Required]
    public string Name { get; set; }

    public bool IsEnabled { get; set; }
    public bool IsSingleton { get; set; }
    public int LockTimeout { get; set; }
    public int LockExpiration { get; set; }
    public bool DeleteFinishedWorkflows { get; set; }

    public bool IsActivity { get; set; }

    public WorkflowBranchingMode BranchingMode { get; set; }

    public bool FaultOnScriptErrors { get; set; }
    public string ReturnUrl { get; set; }
}
