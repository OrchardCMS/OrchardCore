namespace OrchardCore.Jobs;

/// <summary>
/// A unit of work that runs in the background once, whatever the number of servers running the application: a run
/// is executed by a single server, and is recorded with its status, progress, log and result.
/// </summary>
/// <remarks>
/// <para>
/// Register a job with <c>services.AddJob&lt;TJob&gt;(name)</c>, and queue a run of it with
/// <see cref="IJobManager.EnqueueAsync(string, object, Action{JobRequest})"/>. A job is created for each run, in
/// the service scope of that run, so its constructor can take any service of the tenant.
/// </para>
/// <para>
/// Use an <see cref="OrchardCore.BackgroundTasks.IBackgroundTask"/> instead for work that every server has to do,
/// such as cleaning up its local files or its memory.
/// </para>
/// <para>
/// A run may execute more than once, when the job allows several attempts and an attempt fails or its server stops.
/// A job that allows several attempts must then be able to run again from the start.
/// </para>
/// </remarks>
public interface IJob
{
    /// <summary>
    /// Executes a run of the job.
    /// </summary>
    /// <param name="context">The run, its parameters, and the means to report its progress.</param>
    /// <param name="cancellationToken">A token that is cancelled when the run is cancelled, or its server stops.</param>
    /// <returns>A task that completes when the run is done. An exception fails the run.</returns>
    Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
}
