using OrchardCore.Data.Documents;

namespace OrchardCore.Workflows.Trimming.Models;

public class WorkflowTrimmingState : Document
{
    public DateTime? LastRunUtc { get; set; }
}
