using System;
using System.Linq;
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
        Statuses = Enum.GetValues(typeof(WorkflowStatus))
            .Cast<WorkflowStatus>()
            .Select(status => new WorkflowStatusItem
            {
                Status = status,
                IsSelected = false
            })
            .ToArray();
    }
}

public class WorkflowStatusItem
{
    public WorkflowStatus Status { get; set; }

    public bool IsSelected { get; set; }
}
