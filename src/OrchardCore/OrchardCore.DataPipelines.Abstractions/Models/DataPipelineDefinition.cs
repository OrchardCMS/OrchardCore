namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// The steps of a data pipeline and how they are connected. Drafts, published versions and runs each hold their own
/// copy.
/// </summary>
public sealed class DataPipelineDefinition
{
    /// <summary>
    /// Gets or sets the steps of the pipeline.
    /// </summary>
    public List<DataPipelineStep> Steps { get; set; } = [];

    /// <summary>
    /// Gets or sets the connections between the steps.
    /// </summary>
    public List<DataPipelineConnection> Connections { get; set; } = [];

    /// <summary>
    /// Finds a step by its identifier.
    /// </summary>
    /// <param name="stepId">The step identifier.</param>
    /// <returns>The step, or <see langword="null"/> when the pipeline has no such step.</returns>
    public DataPipelineStep FindStep(string stepId)
        => string.IsNullOrEmpty(stepId) ? null : Steps.FirstOrDefault(step => step.StepId == stepId);
}
