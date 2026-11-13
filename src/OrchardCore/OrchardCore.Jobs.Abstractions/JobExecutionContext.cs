using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrchardCore.Jobs.Models;

namespace OrchardCore.Jobs;

/// <summary>
/// What a job receives when a run of it executes: the run, its parameters, the services of its scope, and the means
/// to report its progress, write to its log, set its result and queue child runs.
/// </summary>
public abstract class JobExecutionContext
{
    /// <summary>
    /// Gets the run as it was when it started. Changing it has no effect.
    /// </summary>
    public abstract JobRun Run { get; }

    /// <summary>
    /// Gets the services of the scope the run executes in. What the job saves in this scope is committed when the
    /// job returns, before the run is recorded as succeeded.
    /// </summary>
    public abstract IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the attempt that executes, starting at 1.
    /// </summary>
    public int Attempt => Run.Attempt;

    /// <summary>
    /// Gets the parameters of the run.
    /// </summary>
    /// <typeparam name="TParameters">The type the parameters were queued with.</typeparam>
    /// <returns>The parameters, or a new instance when the run has none.</returns>
    public virtual TParameters GetParameters<TParameters>()
        where TParameters : class, new()
    {
        return Run.Parameters?.Deserialize<TParameters>(JOptions.Default) ?? new TParameters();
    }

    /// <summary>
    /// Adds a message to the log of the run. The log keeps the first <see cref="JobRun.MaxLogEntries"/> messages.
    /// </summary>
    /// <param name="level">The severity of the message.</param>
    /// <param name="message">The message.</param>
    public abstract void Log(LogLevel level, string message);

    /// <summary>
    /// Reports how far the run is. The progress is saved with the next heartbeat of the run, a few seconds later.
    /// </summary>
    /// <param name="percent">The progress from 0 to 100, or <see langword="null"/> when it is unknown.</param>
    /// <param name="message">What the run is doing, if any.</param>
    public abstract void ReportProgress(int? percent, string message = null);

    /// <summary>
    /// Sets the result of the run, which is saved when the run succeeds.
    /// </summary>
    /// <param name="result">An object that is serialized to JSON, or <see langword="null"/>.</param>
    public abstract void SetResult(object result);

    /// <summary>
    /// Queues a run that is a child of this run, such as one batch of its work. Child runs are executed by any
    /// server, and this run only completes once they all completed: it succeeds when they all succeeded.
    /// </summary>
    /// <param name="jobName">The name of the job of the child run.</param>
    /// <param name="parameters">The parameters of the child run, serialized to JSON, or <see langword="null"/>.</param>
    /// <param name="configure">Changes how the child run is queued, if needed.</param>
    /// <returns>The queued child run.</returns>
    public abstract Task<JobRun> EnqueueChildAsync(string jobName, object parameters = null, Action<JobRequest> configure = null);
}

/// <summary>
/// Provides shortcuts to write to the log of a run.
/// </summary>
public static class JobExecutionContextExtensions
{
    /// <summary>
    /// Adds an informational message to the log of the run.
    /// </summary>
    /// <param name="context">The context of the run.</param>
    /// <param name="message">The message.</param>
    public static void LogInformation(this JobExecutionContext context, string message)
        => context.Log(LogLevel.Information, message);

    /// <summary>
    /// Adds a warning to the log of the run.
    /// </summary>
    /// <param name="context">The context of the run.</param>
    /// <param name="message">The message.</param>
    public static void LogWarning(this JobExecutionContext context, string message)
        => context.Log(LogLevel.Warning, message);

    /// <summary>
    /// Adds an error to the log of the run. It doesn't fail the run; throw an exception for that.
    /// </summary>
    /// <param name="context">The context of the run.</param>
    /// <param name="message">The message.</param>
    public static void LogError(this JobExecutionContext context, string message)
        => context.Log(LogLevel.Error, message);
}
