using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.DataPipelines.Workflows;

/// <summary>
/// Queues a run of the published version of a data pipeline. When it waits for the run, the workflow halts and resumes
/// once the run ends, with the run in the <c>DataPipelineRun</c> output and as the last result.
/// </summary>
public sealed class RunDataPipelineTask : TaskActivity<RunDataPipelineTask>
{
    /// <summary>
    /// The name of the input and output that holds the run.
    /// </summary>
    public const string RunKey = "DataPipelineRun";

    private readonly DataPipelineManager _pipelineManager;
    private readonly DataPipelineRunManager _runManager;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizer S;

    public RunDataPipelineTask(
        DataPipelineManager pipelineManager,
        DataPipelineRunManager runManager,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizer<RunDataPipelineTask> localizer)
    {
        _pipelineManager = pipelineManager;
        _runManager = runManager;
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Run Data Pipeline Task"];

    public override LocalizedString Category => S["Data"];

    /// <summary>
    /// Gets or sets the pipeline to run.
    /// </summary>
    public string PipelineId
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the workflow waits for the run to end.
    /// </summary>
    public bool WaitForCompletion
    {
        get => GetProperty(() => true);
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the parameters of the run, one <c>name=value</c> per line, as a Liquid template. Steps read them
    /// with the <c>{Parameter:name}</c> placeholder.
    /// </summary>
    public WorkflowExpression<string> Parameters
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => WaitForCompletion
            ? Outcome(S["Succeeded"], S["Failed"])
            : Outcome(S["Queued"], S["Failed"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var pipeline = await _pipelineManager.GetAsync(PipelineId);

        if (pipeline?.Published is null || !pipeline.IsEnabled)
        {
            workflowContext.LastResult = pipeline is null
                ? S["The data pipeline doesn't exist."].Value
                : S["The data pipeline '{0}' isn't published, or is disabled.", pipeline.Name].Value;

            return Outcome("Failed");
        }

        var activityId = activityContext.ActivityRecord.ActivityId;
        var wait = WaitForCompletion;
        var parameters = string.IsNullOrWhiteSpace(Parameters?.Expression)
            ? null
            : await _expressionEvaluator.EvaluateAsync(Parameters, workflowContext, null);

        var run = await _runManager.QueueAsync(pipeline, new DataPipelineRunRequest
        {
            Trigger = DataPipelineRunRequest.WorkflowTrigger,
            CorrelationId = workflowContext.WorkflowId,
            Parameters = ParseParameters(parameters),
            Alter = run =>
            {
                run.Properties[DataPipelineWorkflowKeys.WorkflowId] = workflowContext.WorkflowId;
                run.Properties[DataPipelineWorkflowKeys.ActivityId] = activityId;
                run.Properties[DataPipelineWorkflowKeys.WaitForCompletion] = wait;
            },
        });

        workflowContext.Properties[RunPropertyName(activityId)] = run.RunId;
        workflowContext.LastResult = run.RunId;

        return wait ? Halt() : Outcome("Queued");
    }

    public override Task<ActivityExecutionResult> ResumeAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var activityId = activityContext.ActivityRecord.ActivityId;

        if (!workflowContext.Input.TryGetValue(RunKey, out var value) || value is not IDictionary<string, object> run ||
            !workflowContext.Properties.TryGetValue(RunPropertyName(activityId), out var runId) ||
            !Equals(run.TryGetValue("RunId", out var resumedRunId) ? resumedRunId?.ToString() : null, runId?.ToString()))
        {
            // Resumed for another run: keep waiting.
            return Task.FromResult(Halt());
        }

        workflowContext.LastResult = run;
        workflowContext.Output[RunKey] = run;

        return Task.FromResult(Outcome(run.TryGetValue("Status", out var status) && status?.ToString() == "Succeeded" ? "Succeeded" : "Failed"));
    }

    private static string RunPropertyName(string activityId) => $"{RunKey}:{activityId}";

    // One name=value per line; a line without a name is ignored.
    private static Dictionary<string, string> ParseParameters(string text)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in (text ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = line.IndexOf('=');

            if (separator > 0)
            {
                parameters[line[..separator].Trim()] = line[(separator + 1)..].Trim();
            }
        }

        return parameters;
    }
}
