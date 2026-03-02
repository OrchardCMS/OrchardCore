namespace OrchardCore.Environment.Shell.Scope;

/// <summary>
/// Provides extension methods for <see cref="ShellScope"/> to simplify common operations such as
/// registering disposal callbacks, adding deferred tasks and signals, handling exceptions,
/// and executing delegates within the scope.
/// </summary>
public static class ShellScopeExtensions
{
    /// <summary>
    /// Registers a delegate task to be invoked before this shell scope will be disposed.
    /// </summary>
    /// <typeparam name="TState">The type of the state object to pass to the callback.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to register the delegate.</param>
    /// <param name="callback">The delegate task to be invoked before disposal. This delegate takes a <see cref="ShellScope"/> and state parameters and returns a <see cref="Task"/>.</param>
    /// <param name="state">The state object to pass to the callback.</param>
    /// <param name="last">A boolean value indicating whether the delegate should be invoked last. 
    /// If true, the delegate is added to the end of the invocation list; otherwise, it is added to the beginning. The default value is false.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope RegisterBeforeDispose<TState>(this ShellScope scope, Func<ShellScope, TState, Task> callback, TState state, bool last = false)
    {
        scope?.BeforeDispose((scope, s) =>
        {
            var (callback, state) = ((Func<ShellScope, TState, Task>, TState))s;

            return callback(scope, state);
        }, (callback, state), last);

        return scope;
    }

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
        scope?.BeforeDispose((scope, s) => ((Func<ShellScope, Task>)s)(scope), callback, last);
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
        scope?.BeforeDispose((scope, s) =>
        {
            ((Action<ShellScope>)s)(scope);
            return Task.CompletedTask;
        }, callback, last);

        return scope;
    }

    /// <summary>
    /// Registers a delegate action to be invoked before this shell scope will be disposed.
    /// </summary>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to register the delegate.</param>
    /// <param name="callback">The delegate action to be invoked before disposal. This delegate takes a <see cref="ShellScope"/> parameter.</param>
    /// <param name="state">The state object to pass to the callback.</param>
    /// <param name="last">A boolean value indicating whether the delegate should be invoked last. 
    /// If true, the delegate is added to the end of the invocation list; otherwise, it is added to the beginning. The default value is false.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope RegisterBeforeDispose<TState>(this ShellScope scope, Action<ShellScope, TState> callback, TState state, bool last = false)
    {
        scope?.BeforeDispose((scope, s) =>
        {
            var (callback, state) = ((Action<ShellScope, TState>, TState))s;
            callback(scope, state);
            return Task.CompletedTask;
        }, (callback, state), last);

        return scope;
    }

    /// <summary>
    /// Adds a Signal (if not already present) to be sent just before this shell scope will be disposed.
    /// </summary>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred signal.</param>
    /// <param name="key">The key identifying the signal to be sent.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredSignal(this ShellScope scope, string key)
    {
        scope?.DeferredSignal(key);
        return scope;
    }

    /// <summary>
    /// Adds a Task to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="TState">The type of the state object to pass to the task.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="task">The delegate to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and state parameters and returns a <see cref="Task"/>.</param>
    /// <param name="state">The state object to pass to the task.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<TState>(this ShellScope scope, Func<ShellScope, TState, Task> task, TState state)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (task, state) = ((Func<ShellScope, TState, Task>, TState))s;
            return task(scope, state);
        }, (task, state));

        return scope;
    }

    /// <summary>
    /// Adds a Task to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="task">The delegate to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and two additional parameters and returns a <see cref="Task"/>.</param>
    /// <param name="arg1">The first argument to pass to the task.</param>
    /// <param name="arg2">The second argument to pass to the task.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<T1, T2>(this ShellScope scope, Func<ShellScope, T1, T2, Task> task, T1 arg1, T2 arg2)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (task, arg1, arg2) = ((Func<ShellScope, T1, T2, Task>, T1, T2))s;
            return task(scope, arg1, arg2);
        }, (task, arg1, arg2));
        return scope;
    }

    /// <summary>
    /// Adds a Task to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="task">The delegate to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and three additional parameters and returns a <see cref="Task"/>.</param>
    /// <param name="arg1">The first argument to pass to the task.</param>
    /// <param name="arg2">The second argument to pass to the task.</param>
    /// <param name="arg3">The third argument to pass to the task.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<T1, T2, T3>(this ShellScope scope, Func<ShellScope, T1, T2, T3, Task> task, T1 arg1, T2 arg2, T3 arg3)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (task, arg1, arg2, arg3) = ((Func<ShellScope, T1, T2, T3, Task>, T1, T2, T3))s;
            return task(scope, arg1, arg2, arg3);
        }, (task, arg1, arg2, arg3));
        return scope;
    }

    /// <summary>
    /// Adds a Task to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="task">The delegate to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and four additional parameters and returns a <see cref="Task"/>.</param>
    /// <param name="arg1">The first argument to pass to the task.</param>
    /// <param name="arg2">The second argument to pass to the task.</param>
    /// <param name="arg3">The third argument to pass to the task.</param>
    /// <param name="arg4">The fourth argument to pass to the task.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<T1, T2, T3, T4>(this ShellScope scope, Func<ShellScope, T1, T2, T3, T4, Task> task, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (task, arg1, arg2, arg3, arg4) = ((Func<ShellScope, T1, T2, T3, T4, Task>, T1, T2, T3, T4))s;
            return task(scope, arg1, arg2, arg3, arg4);
        }, (task, arg1, arg2, arg3, arg4));
        return scope;
    }

    /// <summary>
    /// Adds a Task to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <typeparam name="T5">The type of the fifth argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="task">The delegate to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and five additional parameters and returns a <see cref="Task"/>.</param>
    /// <param name="arg1">The first argument to pass to the task.</param>
    /// <param name="arg2">The second argument to pass to the task.</param>
    /// <param name="arg3">The third argument to pass to the task.</param>
    /// <param name="arg4">The fourth argument to pass to the task.</param>
    /// <param name="arg5">The fifth argument to pass to the task.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<T1, T2, T3, T4, T5>(this ShellScope scope, Func<ShellScope, T1, T2, T3, T4, T5, Task> task, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (task, arg1, arg2, arg3, arg4, arg5) = ((Func<ShellScope, T1, T2, T3, T4, T5, Task>, T1, T2, T3, T4, T5))s;
            return task(scope, arg1, arg2, arg3, arg4, arg5);
        }, (task, arg1, arg2, arg3, arg4, arg5));
        return scope;
    }

    /// <summary>
    /// Adds a Task to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="task">The delegate to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> parameter and returns a <see cref="Task"/>.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask(this ShellScope scope, Func<ShellScope, Task> task)
    {
        scope?.DeferredTask((scope, s) => ((Func<ShellScope, Task>)s)(scope), task);
        return scope;
    }

    /// <summary>
    /// Adds an Action to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="callback">The delegate action to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> parameter.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask(this ShellScope scope, Action<ShellScope> callback)
    {
        scope?.DeferredTask((scope, s) =>
        {
            ((Action<ShellScope>)s)(scope);
            return Task.CompletedTask;
        }, callback);

        return scope;
    }

    /// <summary>
    /// Adds an Action to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="TState">The type of the state object to pass to the callback.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="callback">The delegate action to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and state parameters.</param>
    /// <param name="state">The state object to pass to the callback.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<TState>(this ShellScope scope, Action<ShellScope, TState> callback, TState state)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (callback, state) = ((Action<ShellScope, TState>, TState))s;
            callback(scope, state);
            return Task.CompletedTask;
        }, (callback, state));

        return scope;
    }

    /// <summary>
    /// Adds an Action to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="callback">The delegate action to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and two additional parameters.</param>
    /// <param name="arg1">The first argument to pass to the callback.</param>
    /// <param name="arg2">The second argument to pass to the callback.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<T1, T2>(this ShellScope scope, Action<ShellScope, T1, T2> callback, T1 arg1, T2 arg2)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (callback, arg1, arg2) = ((Action<ShellScope, T1, T2>, T1, T2))s;
            callback(scope, arg1, arg2);
            return Task.CompletedTask;
        }, (callback, arg1, arg2));
        return scope;
    }

    /// <summary>
    /// Adds an Action to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="callback">The delegate action to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and three additional parameters.</param>
    /// <param name="arg1">The first argument to pass to the callback.</param>
    /// <param name="arg2">The second argument to pass to the callback.</param>
    /// <param name="arg3">The third argument to pass to the callback.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<T1, T2, T3>(this ShellScope scope, Action<ShellScope, T1, T2, T3> callback, T1 arg1, T2 arg2, T3 arg3)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (callback, arg1, arg2, arg3) = ((Action<ShellScope, T1, T2, T3>, T1, T2, T3))s;
            callback(scope, arg1, arg2, arg3);
            return Task.CompletedTask;
        }, (callback, arg1, arg2, arg3));
        return scope;
    }

    /// <summary>
    /// Adds an Action to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="callback">The delegate action to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and four additional parameters.</param>
    /// <param name="arg1">The first argument to pass to the callback.</param>
    /// <param name="arg2">The second argument to pass to the callback.</param>
    /// <param name="arg3">The third argument to pass to the callback.</param>
    /// <param name="arg4">The fourth argument to pass to the callback.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<T1, T2, T3, T4>(this ShellScope scope, Action<ShellScope, T1, T2, T3, T4> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (callback, arg1, arg2, arg3, arg4) = ((Action<ShellScope, T1, T2, T3, T4>, T1, T2, T3, T4))s;
            callback(scope, arg1, arg2, arg3, arg4);
            return Task.CompletedTask;
        }, (callback, arg1, arg2, arg3, arg4));
        return scope;
    }

    /// <summary>
    /// Adds an Action to be executed in a new scope once this shell scope has been disposed.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <typeparam name="T5">The type of the fifth argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the deferred task.</param>
    /// <param name="callback">The delegate action to be executed as a deferred task. This delegate takes a <see cref="ShellScope"/> and five additional parameters.</param>
    /// <param name="arg1">The first argument to pass to the callback.</param>
    /// <param name="arg2">The second argument to pass to the callback.</param>
    /// <param name="arg3">The third argument to pass to the callback.</param>
    /// <param name="arg4">The fourth argument to pass to the callback.</param>
    /// <param name="arg5">The fifth argument to pass to the callback.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddDeferredTask<T1, T2, T3, T4, T5>(this ShellScope scope, Action<ShellScope, T1, T2, T3, T4, T5> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        scope?.DeferredTask((scope, s) =>
        {
            var (callback, arg1, arg2, arg3, arg4, arg5) = ((Action<ShellScope, T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5))s;
            callback(scope, arg1, arg2, arg3, arg4, arg5);
            return Task.CompletedTask;
        }, (callback, arg1, arg2, arg3, arg4, arg5));
        return scope;
    }

    /// <summary>
    /// Adds an handler task to be invoked if an exception is thrown while executing in this shell scope.
    /// </summary>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the exception handler.</param>
    /// <param name="handler">The delegate task to be invoked when an exception occurs. This delegate takes a <see cref="ShellScope"/> and an <see cref="Exception"/> parameter and returns a <see cref="Task"/>.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddExceptionHandler(this ShellScope scope, Func<ShellScope, Exception, Task> handler)
    {
        scope?.ExceptionHandler(handler);
        return scope;
    }

    /// <summary>
    /// Adds an handler action to be invoked if an exception is thrown while executing in this shell scope.
    /// </summary>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to add the exception handler.</param>
    /// <param name="handler">The delegate action to be invoked when an exception occurs. This delegate takes a <see cref="ShellScope"/> and an <see cref="Exception"/> parameter.</param>
    /// <returns>The <see cref="ShellScope"/> instance for chaining further calls.</returns>
    public static ShellScope AddExceptionHandler(this ShellScope scope, Action<ShellScope, Exception> handler)
    {
        scope?.ExceptionHandler((scope, e) =>
        {
            handler(scope, e);
            return Task.CompletedTask;
        });

        return scope;
    }

    /// <summary>
    /// Executes a delegate using this shell scope in an isolated async flow,
    /// while managing the shell state and invoking tenant events.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to execute the delegate.</param>
    /// <param name="execute">The delegate to be executed. This delegate takes a <see cref="ShellScope"/> and two additional parameters and returns a <see cref="Task"/>.</param>
    /// <param name="arg1">The first argument to pass to the delegate.</param>
    /// <param name="arg2">The second argument to pass to the delegate.</param>
    /// <param name="activateShell">A boolean value indicating whether the shell should be activated. The default value is true.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task UsingAsync<T1, T2>(this ShellScope scope, Func<ShellScope, T1, T2, Task> execute, T1 arg1, T2 arg2, bool activateShell = true)
        => scope.UsingAsync((scope, state) => state.execute(scope, state.arg1, state.arg2), (execute, arg1, arg2), activateShell);

    /// <summary>
    /// Executes a delegate using this shell scope in an isolated async flow,
    /// while managing the shell state and invoking tenant events.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to execute the delegate.</param>
    /// <param name="execute">The delegate to be executed. This delegate takes a <see cref="ShellScope"/> and three additional parameters and returns a <see cref="Task"/>.</param>
    /// <param name="arg1">The first argument to pass to the delegate.</param>
    /// <param name="arg2">The second argument to pass to the delegate.</param>
    /// <param name="arg3">The third argument to pass to the delegate.</param>
    /// <param name="activateShell">A boolean value indicating whether the shell should be activated. The default value is true.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task UsingAsync<T1, T2, T3>(this ShellScope scope, Func<ShellScope, T1, T2, T3, Task> execute, T1 arg1, T2 arg2, T3 arg3, bool activateShell = true)
        => scope.UsingAsync((scope, state) => state.execute(scope, state.arg1, state.arg2, state.arg3), (execute, arg1, arg2, arg3), activateShell);

    /// <summary>
    /// Executes a delegate using this shell scope in an isolated async flow,
    /// while managing the shell state and invoking tenant events.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to execute the delegate.</param>
    /// <param name="execute">The delegate to be executed. This delegate takes a <see cref="ShellScope"/> and four additional parameters and returns a <see cref="Task"/>.</param>
    /// <param name="arg1">The first argument to pass to the delegate.</param>
    /// <param name="arg2">The second argument to pass to the delegate.</param>
    /// <param name="arg3">The third argument to pass to the delegate.</param>
    /// <param name="arg4">The fourth argument to pass to the delegate.</param>
    /// <param name="activateShell">A boolean value indicating whether the shell should be activated. The default value is true.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task UsingAsync<T1, T2, T3, T4>(this ShellScope scope, Func<ShellScope, T1, T2, T3, T4, Task> execute, T1 arg1, T2 arg2, T3 arg3, T4 arg4, bool activateShell = true)
        => scope.UsingAsync((scope, state) => state.execute(scope, state.arg1, state.arg2, state.arg3, state.arg4), (execute, arg1, arg2, arg3, arg4), activateShell);

    /// <summary>
    /// Executes a delegate using this shell scope in an isolated async flow,
    /// while managing the shell state and invoking tenant events.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <typeparam name="T5">The type of the fifth argument.</typeparam>
    /// <param name="scope">The <see cref="ShellScope"/> instance on which to execute the delegate.</param>
    /// <param name="execute">The delegate to be executed. This delegate takes a <see cref="ShellScope"/> and five additional parameters and returns a <see cref="Task"/>.</param>
    /// <param name="arg1">The first argument to pass to the delegate.</param>
    /// <param name="arg2">The second argument to pass to the delegate.</param>
    /// <param name="arg3">The third argument to pass to the delegate.</param>
    /// <param name="arg4">The fourth argument to pass to the delegate.</param>
    /// <param name="arg5">The fifth argument to pass to the delegate.</param>
    /// <param name="activateShell">A boolean value indicating whether the shell should be activated. The default value is true.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task UsingAsync<T1, T2, T3, T4, T5>(this ShellScope scope, Func<ShellScope, T1, T2, T3, T4, T5, Task> execute, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, bool activateShell = true)
        => scope.UsingAsync((scope, state) => state.execute(scope, state.arg1, state.arg2, state.arg3, state.arg4, state.arg5), (execute, arg1, arg2, arg3, arg4, arg5), activateShell);
}
