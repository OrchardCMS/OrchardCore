using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Jobs;

/// <summary>
/// Queues a run of a job, which a single server executes in the background. When it waits for the run, the workflow
/// halts and resumes once the run ends, with the run in the <c>JobRun</c> output and as the last result. Use it to
/// take long work out of the request or the event that runs the workflow. It runs the jobs that can be run from the
/// admin (<see cref="JobDescriptor.CanRunManually"/>).
/// </summary>
public sealed class RunJobTask : TaskActivity<RunJobTask>, IActivityDescription
{
    /// <summary>
    /// The name of the input and output that holds the run.
    /// </summary>
    public const string RunKey = "JobRun";

    private readonly IJobManager _jobManager;
    private readonly JobOptions _jobOptions;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    private readonly IStringLocalizer S;

    public RunJobTask(
        IJobManager jobManager,
        IOptions<JobOptions> jobOptions,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<RunJobTask> localizer)
    {
        _jobManager = jobManager;
        _jobOptions = jobOptions.Value;
        _expressionEvaluator = expressionEvaluator;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<RunJobTask>("Run Job Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Background;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    /// <summary>
    /// Gets or sets the name of the job to run.
    /// </summary>
    public string JobName
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets what the run is about, shown next to the title of the job, as a Liquid template.
    /// </summary>
    public WorkflowExpression<string> RunTitle
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Gets or sets the parameters of the run, a JSON object, as a Liquid template.
    /// </summary>
    public WorkflowExpression<string> Parameters
    {
        get => GetProperty(() => new WorkflowExpression<string>());
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

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return WaitForCompletion
            ? Outcome(S["Succeeded"], S["Failed"])
            : Outcome(S["Queued"], S["Failed"]);
    }

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var job = _jobOptions.Find(JobName);

        if (job is null)
        {
            workflowContext.LastResult = S["The job '{0}' doesn't exist, such as when its feature is disabled.", JobName].Value;

            return Outcome("Failed");
        }

        // The other jobs only make sense with the parameters their feature queues them with.
        if (!job.CanRunManually)
        {
            workflowContext.LastResult = S["The job '{0}' can't be run from a workflow.", JobName].Value;

            return Outcome("Failed");
        }

        JsonObject parameters = null;

        if (!string.IsNullOrWhiteSpace(Parameters?.Expression))
        {
            var json = await _expressionEvaluator.EvaluateAsync(Parameters, workflowContext, null);

            if (!string.IsNullOrWhiteSpace(json) && !TryParseObject(json, out parameters))
            {
                workflowContext.LastResult = S["The parameters of the job aren't a JSON object."].Value;

                return Outcome("Failed");
            }
        }

        var title = string.IsNullOrWhiteSpace(RunTitle?.Expression)
            ? null
            : await _expressionEvaluator.EvaluateAsync(RunTitle, workflowContext, null);

        var activityId = activityContext.ActivityRecord.ActivityId;
        var wait = WaitForCompletion;

        // The run is saved with the workflow, so it only starts once the workflow that waits for it is saved.
        var run = await _jobManager.EnqueueAsync(job.Name, parameters, request =>
        {
            request.Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
            request.Trigger = JobTriggers.Workflow;
            request.CorrelationId = workflowContext.WorkflowId;
            request.Alter = run =>
            {
                run.Properties[JobWorkflowKeys.WorkflowId] = workflowContext.WorkflowId;
                run.Properties[JobWorkflowKeys.ActivityId] = activityId;
                run.Properties[JobWorkflowKeys.WaitForCompletion] = wait;
            };
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

        return Task.FromResult(Outcome(run.TryGetValue("Status", out var status) && status?.ToString() == nameof(JobRunStatus.Succeeded) ? "Succeeded" : "Failed"));
    }

    internal static bool TryParseObject(string json, out JsonObject jsonObject)
    {
        jsonObject = null;

        try
        {
            jsonObject = JsonNode.Parse(json) as JsonObject;
        }
        catch (JsonException)
        {
        }

        return jsonObject is not null;
    }

    private static string RunPropertyName(string activityId) => $"{RunKey}:{activityId}";
}
