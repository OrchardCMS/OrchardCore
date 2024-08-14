using System;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Trimming.ViewModels;

public class WorkflowTrimmingViewModel
{
    public DateTime? LastRunUtc { get; set; }

    public bool Disabled { get; set; }

    public int RetentionDays { get; set; }

    public WorkflowStatus[] Statuses { get; set; }
}
