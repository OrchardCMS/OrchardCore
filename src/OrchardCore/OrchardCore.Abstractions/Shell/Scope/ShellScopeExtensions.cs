using System;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Scope
{
    public static class ShellScopeExtensions
    {
        /// <summary>
        /// Registers a delegate task to be invoked before this shell scope will be disposed.
        /// </summary>
        public static ShellScope RegisterBeforeDispose(this ShellScope scope, Func<ShellScope, Task> callback)
        {
            scope?.BeforeDispose(callback);
            return scope;
        }

        /// <summary>
        /// Registers a delegate action to be invoked before this shell scope will be disposed.
        /// </summary>
        public static ShellScope RegisterBeforeDispose(this ShellScope scope, Action<ShellScope> callback)
        {
            scope?.BeforeDispose(scope =>
            {
                callback(scope);
                return Task.CompletedTask;
            });

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
}
