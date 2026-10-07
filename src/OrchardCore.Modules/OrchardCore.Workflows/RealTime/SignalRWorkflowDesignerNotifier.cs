using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.RealTime;

/// <summary>
/// Sends the changes of workflow types and instances to the clients of <see cref="WorkflowsHub"/>, once the
/// changes of the current shell scope are committed, so clients that reload see them.
/// </summary>
public sealed class SignalRWorkflowDesignerNotifier : IWorkflowDesignerNotifier
{
    public Task WorkflowTypeChangedAsync(WorkflowTypeChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        ShellScope.AddDeferredTask(scope => SendAsync(scope.ServiceProvider.GetRequiredService<IHubContext<WorkflowsHub>>(), change));

        return Task.CompletedTask;
    }

    public Task InstanceChangedAsync(WorkflowInstanceChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        ShellScope.AddDeferredTask(scope => SendAsync(scope.ServiceProvider.GetRequiredService<IHubContext<WorkflowsHub>>(), change));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a workflow type change to the clients of the workflow type.
    /// </summary>
    public static Task SendAsync(IHubContext<WorkflowsHub> hub, WorkflowTypeChange change)
        => hub.Clients.Group(WorkflowsHub.WorkflowTypeGroup(change.WorkflowTypeId)).SendAsync("WorkflowTypeChanged", new
        {
            kind = change.Kind.ToString(),
            workflowTypeId = change.WorkflowTypeId,
            revision = change.Revision,
            versionId = change.VersionId,
            userId = change.UserId,
            userName = change.UserName,
        });

    /// <summary>
    /// Sends an instance change to the clients of the instance.
    /// </summary>
    public static Task SendAsync(IHubContext<WorkflowsHub> hub, WorkflowInstanceChange change)
        => hub.Clients.Group(WorkflowsHub.InstanceGroup(change.WorkflowId)).SendAsync("InstanceChanged", new
        {
            workflowId = change.WorkflowId,
            workflowTypeId = change.WorkflowTypeId,
            status = change.Status.ToString(),
            isDeleted = change.IsDeleted,
        });
}
