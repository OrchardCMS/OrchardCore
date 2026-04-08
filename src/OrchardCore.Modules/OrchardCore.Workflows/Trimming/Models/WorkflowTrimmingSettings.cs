using OrchardCore.Entities;

namespace OrchardCore.Workflows.Trimming.Models;

public class WorkflowTrimmingSettings : Entity
{
    public int RetentionDays { get; set; } = 90;

    public bool Disabled { get; set; }

    public string[] Statuses { get; set; }
}
