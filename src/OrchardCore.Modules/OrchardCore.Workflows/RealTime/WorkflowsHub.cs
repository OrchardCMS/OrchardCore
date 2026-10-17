using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OrchardCore.Workflows.RealTime;

/// <summary>
/// The hub of the workflow designer and instance pages. Clients subscribe to a workflow type or an instance, and
/// receive its changes (see <see cref="SignalRWorkflowDesignerNotifier"/>). The clients of a workflow type also tell
/// each other who is there: the server relays these messages and keeps no list, so it works on several nodes.
/// </summary>
[Authorize(Policy = PolicyName)]
public sealed class WorkflowsHub : Hub
{
    /// <summary>
    /// The authorization policy of the hub: an authenticated user who can manage workflows.
    /// </summary>
    public const string PolicyName = "WorkflowsHub";

    // The workflow types this connection subscribed to, so leaving tells their groups.
    private const string SubscriptionsKey = "OrchardCore.Workflows.Subscriptions";

    private readonly IAuthorizationService _authorizationService;

    public WorkflowsHub(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Subscribes to the changes of a workflow type, and tells the others there that this user arrived.
    /// </summary>
    public async Task<bool> SubscribeWorkflowType(string workflowTypeId)
    {
        if (string.IsNullOrEmpty(workflowTypeId) || !await CanManageAsync())
        {
            return false;
        }

        var group = WorkflowTypeGroup(workflowTypeId);
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        Subscriptions.Add(workflowTypeId);
        await Clients.OthersInGroup(group).SendAsync("PresenceJoined", CurrentUser());

        return true;
    }

    /// <summary>
    /// Stops the changes of a workflow type, and tells the others there that this user left.
    /// </summary>
    public async Task UnsubscribeWorkflowType(string workflowTypeId)
    {
        if (string.IsNullOrEmpty(workflowTypeId) || !Subscriptions.Remove(workflowTypeId))
        {
            return;
        }

        var group = WorkflowTypeGroup(workflowTypeId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        await Clients.Group(group).SendAsync("PresenceLeft", Context.ConnectionId);
    }

    /// <summary>
    /// Answers a newcomer of a workflow type: tells them that this user is there too.
    /// </summary>
    public Task AnnouncePresence(string workflowTypeId, string connectionId)
    {
        if (string.IsNullOrEmpty(connectionId) || !Subscriptions.Contains(workflowTypeId))
        {
            return Task.CompletedTask;
        }

        return Clients.Client(connectionId).SendAsync("PresenceHere", CurrentUser());
    }

    /// <summary>
    /// Subscribes to the changes of a workflow instance.
    /// </summary>
    public async Task<bool> SubscribeInstance(string workflowId)
    {
        if (string.IsNullOrEmpty(workflowId) || !await CanManageAsync())
        {
            return false;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, InstanceGroup(workflowId));

        return true;
    }

    /// <summary>
    /// Stops the changes of a workflow instance.
    /// </summary>
    public Task UnsubscribeInstance(string workflowId)
        => string.IsNullOrEmpty(workflowId) ? Task.CompletedTask : Groups.RemoveFromGroupAsync(Context.ConnectionId, InstanceGroup(workflowId));

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        foreach (var workflowTypeId in Subscriptions)
        {
            await Clients.Group(WorkflowTypeGroup(workflowTypeId)).SendAsync("PresenceLeft", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// The group of the clients of a workflow type.
    /// </summary>
    public static string WorkflowTypeGroup(string workflowTypeId)
        => $"workflow-type:{workflowTypeId}";

    /// <summary>
    /// The group of the clients of a workflow instance.
    /// </summary>
    public static string InstanceGroup(string workflowId)
        => $"workflow:{workflowId}";

    private HashSet<string> Subscriptions
    {
        get
        {
            if (Context.Items.TryGetValue(SubscriptionsKey, out var value) && value is HashSet<string> subscriptions)
            {
                return subscriptions;
            }

            subscriptions = new HashSet<string>(StringComparer.Ordinal);
            Context.Items[SubscriptionsKey] = subscriptions;

            return subscriptions;
        }
    }

    private Task<bool> CanManageAsync()
        => _authorizationService.AuthorizeAsync(Context.User, WorkflowsPermissions.ManageWorkflows);

    private WorkflowPresence CurrentUser()
        => new()
        {
            ConnectionId = Context.ConnectionId,
            UserId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = Context.User?.Identity?.Name,
        };
}

/// <summary>
/// Someone who has a workflow type open.
/// </summary>
public sealed class WorkflowPresence
{
    public string ConnectionId { get; init; }

    public string UserId { get; init; }

    public string UserName { get; init; }
}
