using Microsoft.Extensions.Logging;

namespace OrchardCore.Modules;

/// <summary>
/// Provides helpers to invoke event handlers while logging non-fatal exceptions.
/// </summary>
public static class InvokeExtensions
{
    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static void Invoke<TEvents>(this IEnumerable<TEvents> events, Action<TEvents> dispatch, ILogger logger)
    {
        foreach (var sink in events)
        {
            try
            {
                dispatch(sink);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
            }
        }
    }

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static void Invoke<TEvents, T1>(this IEnumerable<TEvents> events, Action<TEvents, T1> dispatch, T1 arg1, ILogger logger)
    {
        foreach (var sink in events)
        {
            try
            {
                dispatch(sink, arg1);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
            }
        }
    }

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static IEnumerable<TResult> Invoke<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, TResult> dispatch, ILogger logger)
    {
        var results = new List<TResult>();

        foreach (var sink in events)
        {
            try
            {
                var result = dispatch(sink);
                results.Add(result);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
            }
        }

        return results;
    }

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static IEnumerable<TResult> Invoke<TEvents, T1, TResult>(this IEnumerable<TEvents> events, Func<TEvents, T1, TResult> dispatch, T1 arg1, ILogger logger)
    {
        var results = new List<TResult>();

        foreach (var sink in events)
        {
            try
            {
                var result = dispatch(sink, arg1);
                results.Add(result);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
            }
        }

        return results;
    }

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static IEnumerable<TResult> Invoke<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, IEnumerable<TResult>> dispatch, ILogger logger)
    {
        var results = new List<TResult>();

        foreach (var sink in events)
        {
            try
            {
                var result = dispatch(sink);
                results.AddRange(result);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
            }
        }

        return results;
    }

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task InvokeAsync<TEvents>(this IEnumerable<TEvents> events, Func<TEvents, Task> dispatch, ILogger logger)
        => InvokeAsyncCore(events, dispatch, static (sink, dispatch) => dispatch(sink), logger);

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task InvokeAsync<TEvents, T1>(this IEnumerable<TEvents> events, Func<TEvents, T1, Task> dispatch, T1 arg1, ILogger logger)
        => InvokeAsyncCore(events, (Dispatch: dispatch, Arg1: arg1), static (sink, state) => state.Dispatch(sink, state.Arg1), logger);

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task InvokeAsync<TEvents, T1, T2>(this IEnumerable<TEvents> events, Func<TEvents, T1, T2, Task> dispatch, T1 arg1, T2 arg2, ILogger logger)
        => InvokeAsyncCore(events, (Dispatch: dispatch, Arg1: arg1, Arg2: arg2), static (sink, state) => state.Dispatch(sink, state.Arg1, state.Arg2), logger);

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task InvokeAsync<TEvents, T1, T2, T3>(this IEnumerable<TEvents> events, Func<TEvents, T1, T2, T3, Task> dispatch, T1 arg1, T2 arg2, T3 arg3, ILogger logger)
        => InvokeAsyncCore(events, (Dispatch: dispatch, Arg1: arg1, Arg2: arg2, Arg3: arg3), static (sink, state) => state.Dispatch(sink, state.Arg1, state.Arg2, state.Arg3), logger);

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task InvokeAsync<TEvents, T1, T2, T3, T4>(this IEnumerable<TEvents> events, Func<TEvents, T1, T2, T3, T4, Task> dispatch, T1 arg1, T2 arg2, T3 arg3, T4 arg4, ILogger logger)
        => InvokeAsyncCore(events, (Dispatch: dispatch, Arg1: arg1, Arg2: arg2, Arg3: arg3, Arg4: arg4), static (sink, state) => state.Dispatch(sink, state.Arg1, state.Arg2, state.Arg3, state.Arg4), logger);

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task InvokeAsync<TEvents, T1, T2, T3, T4, T5>(this IEnumerable<TEvents> events, Func<TEvents, T1, T2, T3, T4, T5, Task> dispatch, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, ILogger logger)
        => InvokeAsyncCore(events, (Dispatch: dispatch, Arg1: arg1, Arg2: arg2, Arg3: arg3, Arg4: arg4, Arg5: arg5), static (sink, state) => state.Dispatch(sink, state.Arg1, state.Arg2, state.Arg3, state.Arg4, state.Arg5), logger);

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task<IEnumerable<TResult>> InvokeAsync<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, Task<TResult>> dispatch, ILogger logger)
        => InvokeAsyncResultCore(events, dispatch, static (sink, dispatch) => dispatch(sink), logger);

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task<IEnumerable<TResult>> InvokeAsync<TEvents, T1, TResult>(this IEnumerable<TEvents> events, Func<TEvents, T1, Task<TResult>> dispatch, T1 arg1, ILogger logger)
        => InvokeAsyncResultCore(events, (Dispatch: dispatch, Arg1: arg1), static (sink, state) => state.Dispatch(sink, state.Arg1), logger);

    /// <summary>
    /// Safely invoke methods by catching non fatal exceptions and logging them.
    /// </summary>
    public static Task<IEnumerable<TResult>> InvokeAsync<TEvents, TResult>(this IEnumerable<TEvents> events, Func<TEvents, Task<IEnumerable<TResult>>> dispatch, ILogger logger)
        => InvokeAsyncEnumerableResultCore(events, dispatch, static (sink, dispatch) => dispatch(sink), logger);

    private static Task InvokeAsyncCore<TEvents, TState>(IEnumerable<TEvents> events, TState state, Func<TEvents, TState, Task> dispatch, ILogger logger)
    {
        var enumerator = events.GetEnumerator();
        var disposeEnumerator = true;

        try
        {
            while (enumerator.MoveNext())
            {
                var sink = enumerator.Current;

                try
                {
                    var task = dispatch(sink, state);

                    if (!task.IsCompletedSuccessfully)
                    {
                        disposeEnumerator = false;
                        return InvokeAsyncAwaited(enumerator, sink, task, state, dispatch, logger);
                    }
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                }
            }

            return Task.CompletedTask;
        }
        finally
        {
            if (disposeEnumerator)
            {
                enumerator.Dispose();
            }
        }

        static async Task InvokeAsyncAwaited(IEnumerator<TEvents> enumerator, TEvents sink, Task task, TState state, Func<TEvents, TState, Task> dispatch, ILogger logger)
        {
            using (enumerator)
            {
                while (true)
                {
                    try
                    {
                        await task;
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                    }

                    if (!enumerator.MoveNext())
                    {
                        return;
                    }

                    sink = enumerator.Current;

                    try
                    {
                        task = dispatch(sink, state);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                    }
                }
            }
        }
    }

    private static Task<IEnumerable<TResult>> InvokeAsyncResultCore<TEvents, TState, TResult>(IEnumerable<TEvents> events, TState state, Func<TEvents, TState, Task<TResult>> dispatch, ILogger logger)
    {
        var results = new List<TResult>();
        var enumerator = events.GetEnumerator();
        var disposeEnumerator = true;

        try
        {
            while (enumerator.MoveNext())
            {
                var sink = enumerator.Current;

                try
                {
                    var task = dispatch(sink, state);

                    if (task.IsCompletedSuccessfully)
                    {
                        results.Add(task.Result);
                        continue;
                    }

                    disposeEnumerator = false;
                    return InvokeAsyncResultAwaited(enumerator, sink, task, results, state, dispatch, logger);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                }
            }

            return Task.FromResult<IEnumerable<TResult>>(results);
        }
        finally
        {
            if (disposeEnumerator)
            {
                enumerator.Dispose();
            }
        }

        static async Task<IEnumerable<TResult>> InvokeAsyncResultAwaited(IEnumerator<TEvents> enumerator, TEvents sink, Task<TResult> task, List<TResult> results, TState state, Func<TEvents, TState, Task<TResult>> dispatch, ILogger logger)
        {
            using (enumerator)
            {
                while (true)
                {
                    try
                    {
                        results.Add(await task);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                    }

                    if (!enumerator.MoveNext())
                    {
                        return results;
                    }

                    sink = enumerator.Current;

                    try
                    {
                        task = dispatch(sink, state);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                    }
                }
            }
        }
    }

    private static Task<IEnumerable<TResult>> InvokeAsyncEnumerableResultCore<TEvents, TState, TResult>(IEnumerable<TEvents> events, TState state, Func<TEvents, TState, Task<IEnumerable<TResult>>> dispatch, ILogger logger)
    {
        var results = new List<TResult>();
        var enumerator = events.GetEnumerator();
        var disposeEnumerator = true;

        try
        {
            while (enumerator.MoveNext())
            {
                var sink = enumerator.Current;

                try
                {
                    var task = dispatch(sink, state);

                    if (task.IsCompletedSuccessfully)
                    {
                        results.AddRange(task.Result);
                        continue;
                    }

                    disposeEnumerator = false;
                    return InvokeAsyncEnumerableResultAwaited(enumerator, sink, task, results, state, dispatch, logger);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                }
            }

            return Task.FromResult<IEnumerable<TResult>>(results);
        }
        finally
        {
            if (disposeEnumerator)
            {
                enumerator.Dispose();
            }
        }

        static async Task<IEnumerable<TResult>> InvokeAsyncEnumerableResultAwaited(IEnumerator<TEvents> enumerator, TEvents sink, Task<IEnumerable<TResult>> task, List<TResult> results, TState state, Func<TEvents, TState, Task<IEnumerable<TResult>>> dispatch, ILogger logger)
        {
            using (enumerator)
            {
                while (true)
                {
                    try
                    {
                        results.AddRange(await task);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                    }

                    if (!enumerator.MoveNext())
                    {
                        return results;
                    }

                    sink = enumerator.Current;

                    try
                    {
                        task = dispatch(sink, state);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        ex.LogException(logger, typeof(TEvents), sink.GetType().FullName);
                    }
                }
            }
        }
    }

    [Obsolete("This method rethrows the given exception, which loses the original call stack. It is not recommended for new code. " +
        "Use ExceptionExtensions.LogException instead, which only logs exceptions without rethrowing.")]
    public static void HandleException(Exception ex, ILogger logger, string sourceType, string method)
    {
        if (IsLogged(ex))
        {
            logger.LogError(ex, "{Type} thrown from {Method} by {Exception}",
                sourceType,
                method,
                ex.GetType().Name);
        }

        if (ex.IsFatal())
        {
            throw ex;
        }
    }

    private static bool IsLogged(Exception ex)
    {
        return !ex.IsFatal();
    }
}
