using System;
using OrchardCore.Entities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.WorkflowPruning.Models;

public class WorkflowPruningSettings : Entity
{
    public int RetentionDays { get; set; } = 90;
    public DateTime? LastRunUtc { get; set; }
    public bool Disabled { get; set; }

    public WorkflowStatus[] Statuses { get; set; }
}
