using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.WorkflowPruning.ViewModels;

public class WorkflowPruningViewModel
{
    public DateTime? LastRunUtc { get; set; }

    public bool Disabled { get; set; }

    public int RetentionDays { get; set; }

    public WorkflowStatus[] Statuses { get; set; }
}
