using OrchardCore.Localization;

namespace OrchardCore.Workflows;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Workflows.WorkflowsLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class WorkflowsLocalizationSources
{
    /// <summary>Gets the "All Workflow Types" text.</summary>
    public static readonly LocalizationSource AllWorkflowTypes = LocalizationSource.Create("All Workflow Types", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the "Workflows" text.</summary>
    public static readonly LocalizationSource Workflows = LocalizationSource.Create("Workflows", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the title of the job that retries a workflow instance.</summary>
    public static readonly LocalizationSource RetryJobTitle = LocalizationSource.Create("Workflow Retry", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the description of the job that retries a workflow instance.</summary>
    public static readonly LocalizationSource RetryJobDescription = LocalizationSource.Create("Retries the task of a workflow instance that faulted, when the delay of its retry policy is over.", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the title of the job that runs a workflow.</summary>
    public static readonly LocalizationSource RunWorkflowJobTitle = LocalizationSource.Create("Run Workflow", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the description of the job that runs a workflow.</summary>
    public static readonly LocalizationSource RunWorkflowJobDescription = LocalizationSource.Create("Starts an instance of a workflow. Its parameters are the WorkflowTypeId of the workflow, and optionally the Inputs of its input variables and a CorrelationId.", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the title of the job that triggers the timer events.</summary>
    public static readonly LocalizationSource TimerJobTitle = LocalizationSource.Create("Workflow Timers", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the description of the job that triggers the timer events.</summary>
    public static readonly LocalizationSource TimerJobDescription = LocalizationSource.Create("Starts the workflows that begin with a timer event, and resumes the ones waiting on a timer that is due.", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the title of the trimming job.</summary>
    public static readonly LocalizationSource TrimmingJobTitle = LocalizationSource.Create("Workflow Trimming", typeof(WorkflowsLocalizationSources));

    /// <summary>Gets the description of the trimming job.</summary>
    public static readonly LocalizationSource TrimmingJobDescription = LocalizationSource.Create("Deletes the workflow instances older than the retention period of the workflow trimming settings.", typeof(WorkflowsLocalizationSources));
}
