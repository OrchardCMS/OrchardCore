namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// A problem found in a pipeline, such as a required input left unconnected or a formula that refers to a missing
/// field.
/// </summary>
public sealed class DataPipelineIssue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataPipelineIssue"/> class.
    /// </summary>
    /// <param name="severity">Whether the issue prevents the pipeline from running.</param>
    /// <param name="message">The message shown to the user.</param>
    /// <param name="stepId">The step the issue belongs to, or <see langword="null"/> for the whole pipeline.</param>
    public DataPipelineIssue(DataPipelineIssueSeverity severity, string message, string stepId = null)
    {
        Severity = severity;
        Message = message;
        StepId = stepId;
    }

    /// <summary>
    /// Gets whether the issue prevents the pipeline from running.
    /// </summary>
    public DataPipelineIssueSeverity Severity { get; }

    /// <summary>
    /// Gets the message shown to the user.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the step the issue belongs to, or <see langword="null"/> for the whole pipeline.
    /// </summary>
    public string StepId { get; }
}
