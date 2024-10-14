using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Trimming.ViewModels;

public class WorkflowTrimmingViewModel
{
    public DateTime? LastRunUtc { get; set; }

    public bool Disabled { get; set; }

    public int RetentionDays { get; set; }

    public WorkflowStatusItem[] Statuses { get; set; }

    public WorkflowTrimmingViewModel()
    {
        Statuses = Enum.GetValues<WorkflowStatus>()
            .Cast<WorkflowStatus>()
            .Select(status => new WorkflowStatusItem
            {
                Status = status,
                IsSelected = false
            })
            .ToArray();
    }
}
