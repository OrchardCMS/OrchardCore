using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Reacts to the end of a run, such as resuming the workflow that waits for it. Handlers are called in their own
/// scope after the run is saved.
/// </summary>
public interface IDataPipelineRunHandler
{
    /// <summary>
    /// Called when a run ends, whether it succeeded, failed or was cancelled.
    /// </summary>
    /// <param name="run">The run.</param>
    /// <returns>A task that completes when the handler is done.</returns>
    Task CompletedAsync(DataPipelineRun run);
}
