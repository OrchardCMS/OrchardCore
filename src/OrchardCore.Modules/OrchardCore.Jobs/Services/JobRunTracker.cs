using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Jobs.Services;

/// <summary>
/// Keeps the cancellation sources of the runs executing on this server, so a run can be cancelled at once, and every
/// run stops when the tenant is released or the application stops. Registered as a tenant singleton.
/// </summary>
public sealed class JobRunTracker : IDisposable
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _runs = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _stopping;

    public JobRunTracker(IHostApplicationLifetime applicationLifetime)
    {
        _stopping = CancellationTokenSource.CreateLinkedTokenSource(applicationLifetime.ApplicationStopping);
    }

    /// <summary>
    /// Gets the token that is cancelled when the tenant is released or the application stops.
    /// </summary>
    public CancellationToken Stopping => _stopping.Token;

    /// <summary>
    /// Gets the number of runs executing on this server.
    /// </summary>
    public int Count => _runs.Count;

    /// <summary>
    /// Tracks a run that starts on this server.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <returns>The cancellation source of the run.</returns>
    public CancellationTokenSource Start(string runId)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(_stopping.Token);
        _runs[runId] = source;

        return source;
    }

    /// <summary>
    /// Stops tracking a run that ended.
    /// </summary>
    /// <param name="runId">The run.</param>
    public void Complete(string runId)
    {
        _runs.TryRemove(runId, out _);
    }

    /// <summary>
    /// Cancels a run if it executes on this server.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <returns><see langword="true"/> when the run executes on this server.</returns>
    public bool TryCancel(string runId)
    {
        if (!_runs.TryGetValue(runId, out var source))
        {
            return false;
        }

        try
        {
            source.Cancel();
        }
        catch (ObjectDisposedException)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Tells whether a run executes on this server.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <returns><see langword="true"/> when the run executes on this server.</returns>
    public bool IsRunning(string runId) => _runs.ContainsKey(runId);

    public void Dispose()
    {
        try
        {
            _stopping.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        _stopping.Dispose();
    }
}
