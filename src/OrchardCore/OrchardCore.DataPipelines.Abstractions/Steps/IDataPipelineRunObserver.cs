using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Follows a run as the engine executes it, to record its progress and history.
/// </summary>
public interface IDataPipelineRunObserver
{
    /// <summary>
    /// Called when a step starts.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="metrics">The counters of the step, updated while it runs.</param>
    void StepStarted(DataPipelineStep step, DataPipelineStepMetrics metrics);

    /// <summary>
    /// Called when a step ends.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="status">How the step ended.</param>
    /// <param name="error">The error that made the step fail, if any.</param>
    void StepCompleted(DataPipelineStep step, DataPipelineStepStatus status, Exception error);

    /// <summary>
    /// Called when a message is recorded.
    /// </summary>
    /// <param name="entry">The message.</param>
    void Log(DataPipelineLogEntry entry);
}
