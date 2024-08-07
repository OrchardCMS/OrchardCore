using System;
using System.Linq;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.WorkflowPruning.ViewModels;

public class WorkflowPruningViewModel
{
    public DateTime? LastRunUtc { get; set; }

    public bool Disabled { get; set; }

    public int RetentionDays { get; set; }
    
    public WorkflowStatus[] Statuses { get; set; }
}

public sealed class WorkflowStatusBuilder
{
    public bool IsSelected { get; set; }

    public string Value { get; set; }

    public static WorkflowStatusBuilder[] Build(WorkflowStatus[] selectedStatuses) =>
        Enum.GetValues(typeof(WorkflowStatus))
            .Cast<WorkflowStatus>()
            .Select(x => new WorkflowStatusBuilder
            {
                IsSelected = selectedStatuses?.Contains(x) ?? false,
                Value = x.ToString()
            })
            .OrderBy(x => x.Value)
            .ToArray();
}
