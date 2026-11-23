using OrchardCore.Jobs.Models;

namespace OrchardCore.Jobs;

/// <summary>
/// Handles the events of the runs of every job, such as to resume what waits for a run. Registered in the service
/// container as a scoped service, as a handler usually needs other services.
/// </summary>
public interface IJobRunHandler
{
    /// <summary>
    /// Called once a run ended: it succeeded, it failed on its last attempt, or it was cancelled. It is called on
    /// the server that recorded the end of the run, in a scope of its own.
    /// </summary>
    /// <param name="run">The run that ended.</param>
    /// <returns>A task that completes when the event is handled. An exception is logged, and doesn't change the run.</returns>
    Task CompletedAsync(JobRun run);
}
