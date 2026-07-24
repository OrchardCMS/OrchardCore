#nullable enable

using System.Collections.Concurrent;

namespace OrchardCore.Media.Processing;

/// <summary>
/// Coalesces concurrent asynchronous work that shares the same key so the underlying factory runs
/// only once per key at a time. Callers that arrive while a worker is already running for a key await
/// the same result instead of starting redundant work. This provides cache-stampede ("thundering
/// herd") protection: a burst of cold-cache requests for the same resized image performs the
/// expensive transform a single time while every other request waits on that one operation.
/// </summary>
/// <typeparam name="TKey">The type of the key that identifies a unit of work.</typeparam>
/// <typeparam name="TValue">The type of the value produced by the work.</typeparam>
internal sealed class SingleFlight<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Task<TValue?>> _workers = new();

    /// <summary>
    /// Runs <paramref name="valueFactory"/> for <paramref name="key"/> unless an identical operation
    /// is already in flight, in which case the existing operation's result is awaited and returned.
    /// </summary>
    public async Task<TValue?> ScheduleAsync(TKey key, Func<TKey, Task<TValue?>> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);

        while (true)
        {
            if (_workers.TryGetValue(key, out var task))
            {
                return await task;
            }

            // This is the task that is returned to all waiters. It completes when the factory completes.
            var tcs = new TaskCompletionSource<TValue?>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (_workers.TryAdd(key, tcs.Task))
            {
                try
                {
                    var value = await valueFactory(key);
                    tcs.TrySetResult(value);
                    return await tcs.Task;
                }
                catch (Exception ex)
                {
                    // Make sure all waiters observe the exception.
                    tcs.SetException(ex);

                    throw;
                }
                finally
                {
                    // Remove the entry so a failed factory is not cached permanently and future
                    // requests can retry.
                    _workers.TryRemove(key, out _);
                }
            }
        }
    }

    /// <summary>
    /// Runs <paramref name="valueFactory"/> for <paramref name="key"/> unless an identical operation
    /// is already in flight, in which case the existing operation's result is awaited and returned.
    /// The <paramref name="state"/> overload avoids capturing a closure on the hot path.
    /// </summary>
    public async Task<TValue?> ScheduleAsync<TState>(TKey key, TState state, Func<TKey, TState, Task<TValue?>> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);

        while (true)
        {
            if (_workers.TryGetValue(key, out var task))
            {
                return await task;
            }

            // This is the task that is returned to all waiters. It completes when the factory completes.
            var tcs = new TaskCompletionSource<TValue?>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (_workers.TryAdd(key, tcs.Task))
            {
                try
                {
                    var value = await valueFactory(key, state);
                    tcs.TrySetResult(value);
                    return await tcs.Task;
                }
                catch (Exception ex)
                {
                    // Make sure all waiters observe the exception.
                    tcs.SetException(ex);

                    throw;
                }
                finally
                {
                    // Remove the entry so a failed factory is not cached permanently and future
                    // requests can retry.
                    _workers.TryRemove(key, out _);
                }
            }
        }
    }
}
