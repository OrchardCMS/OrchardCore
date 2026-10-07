using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.RealTime;

/// <summary>
/// Whether the <c>OrchardCore.Workflows.SignalR</c> feature is enabled.
/// </summary>
public static class WorkflowsRealTime
{
    /// <summary>
    /// The path of <see cref="WorkflowsHub"/>, under the tenant's path.
    /// </summary>
    public const string HubPath = "/hubs/workflows";

    /// <summary>
    /// Whether the pages connect to <see cref="WorkflowsHub"/>. SignalR's hub contexts resolve for any hub once
    /// <c>OrchardCore.SignalR</c> is enabled, so this checks the feature's notifier instead.
    /// </summary>
    public static bool IsEnabled(IServiceProvider services)
        => services?.GetService<IWorkflowDesignerNotifier>() is SignalRWorkflowDesignerNotifier;
}
