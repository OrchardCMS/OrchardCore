namespace OrchardCore.Environment.Shell.Scope;

public static class ShellScopeExtensions
{
    /// <summary>
    /// Registers a delegate task to be invoked before this shell scope will be disposed.
    /// </summary>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to register the delegate.</param>
    /// <param name="callback">The delegate task to be invoked before disposal. This delegate takes a <see cref="ShellScope"/> parameter and returns a <see cref="Task"/>.</param>
    /// <param name="last">A boolean value indicating whether the delegate should be invoked last. 
    /// If true, the delegate is added to the end of the invocation list; otherwise, it is added to the beginning. The default value is false.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope RegisterBeforeDispose(this ShellScope scope, Func<ShellScope, Task> callback, bool last = false)
    {
        scope?.BeforeDispose(callback, last);

        return scope;
    }

    /// <summary>
    /// Registers a delegate action to be invoked before this shell scope will be disposed.
    /// </summary>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to register the delegate.</param>
    /// <param name="callback">The delegate action to be invoked before disposal. This delegate takes a <see cref="ShellScope"/> parameter.</param>
    /// <param name="last">A boolean value indicating whether the delegate should be invoked last. 
    /// If true, the delegate is added to the end of the invocation list; otherwise, it is added to the beginning. The default value is false.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope RegisterBeforeDispose(this ShellScope scope, Action<ShellScope> callback, bool last = false)
    {
        scope?.BeforeDispose(scope =>
        {
            callback(scope);
            return Task.CompletedTask;
        }, last);

        return scope;
    }

    /// <summary>
    /// Adds a Signal (if not already present) to be sent just before this shell scope will be disposed.
    /// </summary>
    public static ShellScope AddDeferredSignal(this ShellScope scope, string key)
    {
        scope?.DeferredSignal(key);
        return scope;
    }

    /// <summary>
    /// Adds a Task to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    public static ShellScope AddDeferredTask(this ShellScope scope, Func<ShellScope, Task> task)
    {
        scope?.DeferredTask(task);
        return scope;
    }

    /// <summary>
    /// Adds an Action to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    public static ShellScope AddDeferredTask(this ShellScope scope, Action<ShellScope> callback)
    {
        scope?.DeferredTask(scope =>
        {
            callback(scope);
            return Task.CompletedTask;
        });

        return scope;
    }

    /// <summary>
    /// Adds an handler task to be invoked if an exception is thrown while executing in this shell scope.
    /// </summary>
    public static ShellScope AddExceptionHandler(this ShellScope scope, Func<ShellScope, Exception, Task> handler)
    {
        scope?.ExceptionHandler(handler);
        return scope;
    }

    /// <summary>
    /// Adds an handler action to be invoked if an exception is thrown while executing in this shell scope.
    /// </summary>
    public static ShellScope AddExceptionHandler(this ShellScope scope, Action<ShellScope, Exception> handler)
    {
        scope?.ExceptionHandler((scope, e) =>
        {
            handler(scope, e);
            return Task.CompletedTask;
        });

        return scope;
    }
}
