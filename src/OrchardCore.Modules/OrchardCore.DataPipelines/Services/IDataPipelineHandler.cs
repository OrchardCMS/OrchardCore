using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Reacts to changes of pipelines, such as to prepare what a published pipeline reads, or to remove what a deleted one
/// left behind. Register an implementation as a scoped service. The handlers are called in the scope of the change,
/// before it is committed.
/// </summary>
public interface IDataPipelineHandler
{
    /// <summary>
    /// Called when a pipeline is published, whether a new version was created or not.
    /// </summary>
    /// <param name="pipeline">The pipeline, with its published definition.</param>
    /// <param name="version">The published version.</param>
    /// <returns>A task that completes when the handler is done.</returns>
    Task PublishedAsync(DataPipeline pipeline, DataPipelineVersion version) => Task.CompletedTask;

    /// <summary>
    /// Called when a pipeline is deleted, with its versions, its runs and its state.
    /// </summary>
    /// <param name="pipeline">The deleted pipeline.</param>
    /// <returns>A task that completes when the handler is done.</returns>
    Task DeletedAsync(DataPipeline pipeline) => Task.CompletedTask;
}
