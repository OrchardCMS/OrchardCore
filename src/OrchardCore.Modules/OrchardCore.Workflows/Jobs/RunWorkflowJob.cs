using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Jobs;
using OrchardCore.Locking.Distributed;
using OrchardCore.Workflows.Events;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Jobs;

/// <summary>
/// The parameters of a run of <see cref="RunWorkflowJob"/>.
/// </summary>
public sealed class RunWorkflowJobParameters
{
    /// <summary>
    /// Gets or sets the identifier of the workflow to start.
    /// </summary>
    public string WorkflowTypeId { get; set; }

    /// <summary>
    /// Gets or sets the values of the input variables of the workflow, by name.
    /// </summary>
    public JsonObject Inputs { get; set; }

    /// <summary>
    /// Gets or sets a value to associate the workflow instance with, such as the identifier of a content item.
    /// </summary>
    public string CorrelationId { get; set; }
}

/// <summary>
/// Starts an instance of a workflow in the background, on a single server whatever their number. Schedule it to run
/// a workflow at the occurrences of a cron expression, or queue it to take a workflow out of a request. The workflow
/// starts on its Started By Workflow event when it has one, with its input variables set from the parameters of the
/// run, else on its first start event.
/// </summary>
public sealed class RunWorkflowJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "RunWorkflow";

    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowStore _workflowStore;
    private readonly IWorkflowManager _workflowManager;
    private readonly IDistributedLock _distributedLock;

    public RunWorkflowJob(
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowStore workflowStore,
        IWorkflowManager workflowManager,
        IDistributedLock distributedLock)
    {
        _workflowTypeStore = workflowTypeStore;
        _workflowStore = workflowStore;
        _workflowManager = workflowManager;
        _distributedLock = distributedLock;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<RunWorkflowJobParameters>();

        var workflowType = string.IsNullOrEmpty(parameters.WorkflowTypeId) ? null : await _workflowTypeStore.GetAsync(parameters.WorkflowTypeId);

        if (workflowType is null)
        {
            throw new InvalidOperationException($"The workflow '{parameters.WorkflowTypeId}' doesn't exist. Set the 'WorkflowTypeId' parameter of the run to the identifier of a workflow.");
        }

        if (!workflowType.IsEnabled)
        {
            throw new InvalidOperationException($"The workflow '{workflowType.Name}' is disabled.");
        }

        var start = workflowType.Activities.FirstOrDefault(activity => activity.IsStart && activity.Name == nameof(StartedByWorkflowEvent))
            ?? workflowType.Activities.FirstOrDefault(activity => activity.IsStart)
            ?? throw new InvalidOperationException($"The workflow '{workflowType.Name}' has no start event.");

        // As the other ways to start an instance do, a singleton is checked and started under the lock of its type.
        var (locker, locked) = await _distributedLock.TryAcquireWorkflowTypeLockAsync(workflowType);

        if (!locked)
        {
            throw new InvalidOperationException($"The workflow '{workflowType.Name}' runs one instance at a time, and one is starting.");
        }

        await using var acquiredLock = locker;

        if (workflowType.IsSingleton && await _workflowStore.HasHaltedInstanceAsync(workflowType.WorkflowTypeId))
        {
            context.LogWarning($"The workflow '{workflowType.Name}' runs one instance at a time, and one is running: no instance was started.");

            return;
        }

        var input = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, value) in parameters.Inputs ?? [])
        {
            input[name] = ToValue(value);
        }

        var workflowContext = await _workflowManager.StartWorkflowAsync(workflowType, start, input, parameters.CorrelationId);

        context.LogInformation($"An instance of the workflow '{workflowType.Name}' was started, and is now {workflowContext.Status}.");
        context.SetResult(new
        {
            workflowContext.Workflow.WorkflowId,
            Status = workflowContext.Status.ToString(),
            workflowContext.Workflow.FaultMessage,
        });

        if (workflowContext.Status == WorkflowStatus.Faulted && workflowContext.Workflow.PendingRetry is null)
        {
            throw new InvalidOperationException($"The instance of the workflow '{workflowType.Name}' faulted: {workflowContext.Workflow.FaultMessage}");
        }
    }

    // The simple values are passed as .NET values, which the input variables of the workflow convert to their type.
    private static object ToValue(JsonNode node)
    {
        if (node is not JsonValue value)
        {
            return node?.DeepClone();
        }

        return value.GetValueKind() switch
        {
            JsonValueKind.String => value.GetValue<string>(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => value.TryGetValue<long>(out var number) ? number : value.GetValue<double>(),
            _ => null,
        };
    }
}
