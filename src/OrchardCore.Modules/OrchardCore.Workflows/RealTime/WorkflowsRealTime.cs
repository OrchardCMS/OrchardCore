using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.RealTime;

/// <summary>
/// Whether the workflow pages update live, which they do when <c>OrchardCore.SignalR</c> is enabled.
/// </summary>
public static class WorkflowsRealTime
{
    /// <summary>
    /// The path of <see cref="WorkflowsHub"/>, under the tenant's path.
    /// </summary>
    public const string HubPath = "/hubs/workflows";

    /// <summary>
    /// Whether the pages connect to <see cref="WorkflowsHub"/>: whether the real-time notifier is registered, which it
    /// is when <c>OrchardCore.SignalR</c> is enabled.
    /// </summary>
    public static bool IsEnabled(IServiceProvider services)
        => services?.GetService<IWorkflowDesignerNotifier>() is SignalRWorkflowDesignerNotifier;
}
