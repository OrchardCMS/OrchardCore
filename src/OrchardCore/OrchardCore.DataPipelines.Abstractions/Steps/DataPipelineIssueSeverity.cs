namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Identifies how serious a <see cref="DataPipelineIssue"/> is.
/// </summary>
public enum DataPipelineIssueSeverity
{
    /// <summary>
    /// The pipeline can run, but probably not as intended.
    /// </summary>
    Warning,

    /// <summary>
    /// The pipeline can't run, and can't be published.
    /// </summary>
    Error,
}
