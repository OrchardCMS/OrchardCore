namespace OrchardCore.Workflows.Services;

/// <summary>
/// The notifier used without real-time updates: it does nothing.
/// </summary>
public sealed class NullWorkflowDesignerNotifier : IWorkflowDesignerNotifier
{
    public Task WorkflowTypeChangedAsync(WorkflowTypeChange change)
        => Task.CompletedTask;

    public Task InstanceChangedAsync(WorkflowInstanceChange change)
        => Task.CompletedTask;
}
