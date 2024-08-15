using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.WorkflowPruning.Services;

internal sealed class WorkflowStatusBuilder
{
    public bool IsSelected { get; set; }

    public string Value { get; set; }

    public static WorkflowStatusBuilder[] Build(WorkflowStatus[] selectedStatuses) =>
        Enum.GetValues<WorkflowStatus>()
            .Cast<WorkflowStatus>()
            .Select(x => new WorkflowStatusBuilder
            {
                IsSelected = selectedStatuses?.Contains(x) ?? false,
                Value = x.ToString()
            })
            .OrderBy(x => x.Value)
            .ToArray();
}
