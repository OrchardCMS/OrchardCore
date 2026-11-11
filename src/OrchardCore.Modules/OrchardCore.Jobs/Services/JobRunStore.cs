using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using YesSql;

namespace OrchardCore.Jobs.Services;

/// <summary>
/// What happened when a run was changed.
/// </summary>
internal enum JobRunUpdateOutcome
{
    /// <summary>
    /// The run was changed and saved.
    /// </summary>
    Saved,

    /// <summary>
    /// The run doesn't exist anymore.
    /// </summary>
    NotFound,

    /// <summary>
    /// The change didn't apply to the run as it is now.
    /// </summary>
    Refused,

    /// <summary>
    /// Another scope, possibly on another server, changed the run at the same time.
    /// </summary>
    Conflict,
}

/// <summary>
/// Reads and changes runs, each time in a scope and a database session of its own, so a change is committed at once,
/// whatever the caller does with its own session.
/// </summary>
/// <remarks>
/// Every change is saved with an optimistic concurrency check: the run is only saved if nobody changed it since it
/// was read. That is what lets a single server take a run, without a distributed lock: when two servers change a
/// queued run into a running one at the same time, one of them gets a conflict.
/// </remarks>
internal static class JobRunStore
{
    private const int MaxAttempts = 5;

    /// <summary>
    /// Reads a run in a scope of its own, so the caller gets what is committed.
    /// </summary>
    public static async Task<JobRun> LoadAsync(string runId)
    {
        JobRun run = null;

        await ShellScope.UsingChildScopeAsync(async scope =>
        {
            run = await QueryAsync(scope.ServiceProvider.GetRequiredService<ISession>(), runId);
        });

        return run;
    }

    /// <summary>
    /// Reads a run with a session.
    /// </summary>
    public static Task<JobRun> QueryAsync(ISession session, string runId)
    {
        return session.Query<JobRun, JobRunIndex>(index => index.RunId == runId, collection: JobRun.Collection).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Lists the child runs of a run in a scope of its own, the oldest first.
    /// </summary>
    public static async Task<IReadOnlyList<JobRun>> LoadChildrenAsync(string parentRunId)
    {
        IReadOnlyList<JobRun> children = [];

        await ShellScope.UsingChildScopeAsync(async scope =>
        {
            children = await QueryChildrenAsync(scope.ServiceProvider.GetRequiredService<ISession>(), parentRunId);
        });

        return children;
    }

    /// <summary>
    /// Lists the child runs of a run with a session, the oldest first.
    /// </summary>
    public static async Task<IReadOnlyList<JobRun>> QueryChildrenAsync(ISession session, string parentRunId)
    {
        var children = await session.Query<JobRun, JobRunIndex>(index => index.ParentRunId == parentRunId, collection: JobRun.Collection)
            .OrderBy(index => index.CreatedUtc)
            .ListAsync();

        return children.ToList();
    }

    /// <summary>
    /// Changes a run in a scope of its own, and tries again when another scope changed it at the same time.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <param name="change">Changes the run, and returns <see langword="false"/> when the change doesn't apply.</param>
    /// <returns>The run as it was saved, or <see langword="null"/>, and what happened.</returns>
    public static async Task<(JobRun Run, JobRunUpdateOutcome Outcome)> UpdateAsync(string runId, Func<JobRun, bool> change)
    {
        var result = (Run: (JobRun)null, Outcome: JobRunUpdateOutcome.Conflict);

        for (var attempt = 0; attempt < MaxAttempts && result.Outcome == JobRunUpdateOutcome.Conflict; attempt++)
        {
            result = await TryUpdateAsync(runId, (session, run) => Task.FromResult(change(run)));
        }

        return result;
    }

    /// <summary>
    /// Changes a run in a scope of its own, once: a conflict means another scope changed the run at the same time.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <param name="change">Changes the run, and returns <see langword="false"/> when the change doesn't apply. It
    /// gets the session the run was read with, to check other runs first.</param>
    /// <returns>The run as it was saved, or <see langword="null"/>, and what happened.</returns>
    public static async Task<(JobRun Run, JobRunUpdateOutcome Outcome)> TryUpdateAsync(string runId, Func<ISession, JobRun, Task<bool>> change)
    {
        var result = (Run: (JobRun)null, Outcome: JobRunUpdateOutcome.NotFound);

        await ShellScope.UsingChildScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var run = await QueryAsync(session, runId);

            if (run is null)
            {
                return;
            }

            if (!await change(session, run))
            {
                result = (run, JobRunUpdateOutcome.Refused);

                return;
            }

            await session.SaveAsync(run, checkConcurrency: true, collection: JobRun.Collection);

            try
            {
                // Commit now, to know here whether the run was changed by another scope in between.
                await session.SaveChangesAsync();
                result = (run, JobRunUpdateOutcome.Saved);
            }
            catch (ConcurrencyException)
            {
                result = (null, JobRunUpdateOutcome.Conflict);
            }
        });

        return result;
    }

    /// <summary>
    /// Deletes a run in a scope of its own.
    /// </summary>
    public static Task DeleteAsync(string runId)
    {
        return ShellScope.UsingChildScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var run = await QueryAsync(session, runId);

            if (run is not null)
            {
                session.Delete(run, collection: JobRun.Collection);
            }
        });
    }

    /// <summary>
    /// Adds a message to the log of a run, unless the log is full.
    /// </summary>
    public static void AddLog(JobRun run, DateTime utc, Microsoft.Extensions.Logging.LogLevel level, string message)
    {
        if (run.Log.Count < JobRun.MaxLogEntries)
        {
            run.Log.Add(new JobLogEntry { Utc = utc, Level = level, Message = message });
        }
    }
}
