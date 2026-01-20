using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Trimming.ViewModels;

public class WorkflowStatusItem
{
    public WorkflowStatus Status { get; set; }

    public bool IsSelected { get; set; }
}

