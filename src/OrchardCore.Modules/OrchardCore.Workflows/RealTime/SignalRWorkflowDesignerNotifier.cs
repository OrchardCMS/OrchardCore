using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.RealTime;

/// <summary>
/// Sends the changes of workflow types and instances to the clients of <see cref="WorkflowsHub"/>, once the
/// changes of the current shell scope are committed, so clients that reload see them. The changes of a scope are
/// sent together, with the last change of each instance only.
/// </summary>
public sealed class SignalRWorkflowDesignerNotifier : IWorkflowDesignerNotifier
{
    private readonly List<WorkflowTypeChange> _typeChanges = [];
    private readonly Dictionary<string, WorkflowInstanceChange> _instanceChanges = new(StringComparer.Ordinal);
    private bool _isSendDeferred;

    public Task WorkflowTypeChangedAsync(WorkflowTypeChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        _typeChanges.Add(change);
        DeferSend();

        return Task.CompletedTask;
    }

    public Task InstanceChangedAsync(WorkflowInstanceChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        // Saving an instance several times in a scope sends only what it ended up as.
        _instanceChanges[change.WorkflowId] = change;
        DeferSend();

        return Task.CompletedTask;
    }

    private void DeferSend()
    {
        if (_isSendDeferred || ShellScope.Current is null)
        {
            return;
        }

        _isSendDeferred = true;
        ShellScope.AddDeferredTask(SendChangesAsync);
    }

    private async Task SendChangesAsync(ShellScope scope)
    {
        var typeChanges = _typeChanges.ToArray();
        var instanceChanges = _instanceChanges.Values.ToArray();

        // A change made after the send is sent with the next deferred task.
        _typeChanges.Clear();
        _instanceChanges.Clear();
        _isSendDeferred = false;

        var hub = scope.ServiceProvider.GetRequiredService<IHubContext<WorkflowsHub>>();

        foreach (var change in typeChanges)
        {
            await SendAsync(hub, change);
        }

        foreach (var change in instanceChanges)
        {
            await SendAsync(hub, change);
        }
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
