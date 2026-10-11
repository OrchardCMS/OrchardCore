using System.Text.Json.Nodes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Workflows.Services;

namespace OrchardCore.DataPipelines.Workflows;

/// <summary>
/// When a run ends, resumes the workflow that waits for it, and triggers the <see cref="DataPipelineRunCompletedEvent"/>.
/// </summary>
public sealed class WorkflowDataPipelineRunHandler : IDataPipelineRunHandler
{
    private readonly IWorkflowManager _workflowManager;
    private readonly IWorkflowStore _workflowStore;

    public WorkflowDataPipelineRunHandler(IWorkflowManager workflowManager, IWorkflowStore workflowStore)
    {
        _workflowManager = workflowManager;
        _workflowStore = workflowStore;
    }

    public async Task CompletedAsync(DataPipelineRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        var input = new Dictionary<string, object> { [RunDataPipelineTask.RunKey] = ToDictionary(run) };

        if (run.Properties[DataPipelineWorkflowKeys.WaitForCompletion] is JsonValue wait && wait.TryGetValue<bool>(out var waits) && waits &&
            run.Properties[DataPipelineWorkflowKeys.WorkflowId]?.GetValue<string>() is { Length: > 0 } workflowId &&
            run.Properties[DataPipelineWorkflowKeys.ActivityId]?.GetValue<string>() is { Length: > 0 } activityId)
        {
            var workflow = await _workflowStore.GetAsync(workflowId);
            var blocking = workflow?.BlockingActivities.FirstOrDefault(activity => activity.ActivityId == activityId);

            if (blocking is not null)
            {
                await _workflowManager.ResumeWorkflowAsync(workflow, blocking, input);
            }
        }

        await _workflowManager.TriggerEventAsync(nameof(DataPipelineRunCompletedEvent), input, correlationId: run.PipelineId);
    }

    private static Dictionary<string, object> ToDictionary(DataPipelineRun run)
        => new(StringComparer.Ordinal)
        {
            ["RunId"] = run.RunId,
            ["PipelineId"] = run.PipelineId,
            ["PipelineName"] = run.PipelineName,
            ["Status"] = run.Status.ToString(),
            ["Error"] = run.Error,
            ["CompletedUtc"] = run.CompletedUtc,
            ["Deliveries"] = run.Deliveries
                .Select(delivery => new Dictionary<string, object> { ["Description"] = delivery.Description, ["Url"] = delivery.Url })
                .ToList(),
            ["Steps"] = run.Steps
                .Select(step => new Dictionary<string, object>
                {
                    ["StepId"] = step.StepId,
                    ["Title"] = step.Title ?? step.Type,
                    ["Status"] = step.Status.ToString(),
                    ["RowsIn"] = step.RowsIn,
                    ["RowsOut"] = step.RowsOut,
                })
                .ToList(),
        };
}
