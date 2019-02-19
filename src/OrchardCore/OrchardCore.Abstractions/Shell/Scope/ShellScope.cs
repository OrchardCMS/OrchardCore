using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Scope
{
    /// <summary>
    /// Custom 'IServiceScope' managing the shell state and the execution flow.
    /// </summary>
    public class ShellScope : IServiceScope
    {
        private readonly IServiceScope _serviceScope;

        private bool _disposed = false;
        internal bool _disposeShellContext = false;

        private static AsyncLocal<ShellScope> _current = new AsyncLocal<ShellScope>();
        private List<Func<ShellScope, Task>> _beforeDispose { get; set; } = new List<Func<ShellScope, Task>>();
        private List<Func<ShellScope, Task>> _deferredTasks { get; set; } = new List<Func<ShellScope, Task>>();
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ShellScope(ShellContext shellContext)
        {
            // Prevent the context from being disposed until the end of the scope
            Interlocked.Increment(ref shellContext._refCount);

            ShellContext = shellContext;

            // The service provider is null if we try to create
            // a scope on a disabled shell or already disposed.
            if (shellContext.ServiceProvider == null)
            {
                throw new ArgumentNullException(nameof(shellContext.ServiceProvider), $"Can't resolve a scope on tenant: {shellContext.Settings.Name}");
            }

            _serviceScope = shellContext.ServiceProvider.CreateScope();
            ServiceProvider = _serviceScope.ServiceProvider;
        }

        public ShellContext ShellContext { get; }
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Retrieve the 'ShellContext' of the current shell scope.
        /// </summary>
        public static ShellContext Context
        {
            get => Current?.ShellContext;
        }

        /// <summary>
        /// Retrieve the 'IServiceProvider' of the current shell scope.
        /// </summary>
        public static IServiceProvider Services
        {
            get => Current?.ServiceProvider;
        }

        /// <summary>
        /// Retrieve the current shell scope from the async flow.
        /// </summary>
        public static ShellScope Current
        {
            get => _current.Value;
        }

        /// <summary>
        /// Start holding this shell scope along the async flow.
        /// </summary>
        private void StartAsyncFlow() => _current.Value = this;

        /// <summary>
        /// Execute a delegate using this shell scope.
        /// </summary>
        public async Task UsingAsync(Func<ShellScope, Task> execute)
        {
            if (Current == this)
            {
                await execute(Current);
                return;
            }

            using (this)
            {
                StartAsyncFlow();

                await ActivateShellAsync();

                await execute(this);

                await BeforeDisposeAsync();
            }
        }

        /// <summary>
        /// Activate the shell, if not yet done, by calling the related tenant event handlers.
        /// </summary>
        public async Task ActivateShellAsync()
        {
            if (ShellContext.IsActivated)
            {
                return;
            }

            var semaphore = _semaphores.GetOrAdd(ShellContext.Settings.Name, (name) => new SemaphoreSlim(1));

            await semaphore.WaitAsync();

            try
            {
                // The tenant gets activated here.
                if (!ShellContext.IsActivated)
                {
                    using (var scope = ShellContext.CreateScope())
                    {
                        if (scope == null)
                        {
                            return;
                        }

                        scope.StartAsyncFlow();

                        var tenantEvents = scope.ServiceProvider.GetServices<IModularTenantEvents>();

                        foreach (var tenantEvent in tenantEvents)
                        {
                            await tenantEvent.ActivatingAsync();
                        }

                        foreach (var tenantEvent in tenantEvents.Reverse())
                        {
                            await tenantEvent.ActivatedAsync();
                        }

                        await scope.BeforeDisposeAsync();
                    }

                    ShellContext.IsActivated = true;
                }
            }

            finally
            {
                semaphore.Release();
                _semaphores.TryRemove(ShellContext.Settings.Name, out semaphore);
            }
        }

        /// <summary>
        /// Registers a delegate to be invoked when 'BeforeDisposeAsync()' is called on this scope.
        /// </summary>
        public void RegisterBeforeDispose(Func<ShellScope, Task> callback) => _beforeDispose.Add(callback);

        /// <summary>
        /// Registers a Task to be executed in a new scope at the end of 'BeforeDisposeAsync()'.
        /// </summary>
        public void RegisterDeferredTask(Func<ShellScope, Task> task) => _deferredTasks.Add(task);

        /// <summary>
        /// Adds a delegate to be invoked just before the current shell scope will be disposed.
        /// </summary>
        public static void AddBeforeDispose(Func<ShellScope, Task> callback) => Current?.RegisterBeforeDispose(callback);

        /// <summary>
        /// Adds a Task to be executed in a new scope once the current shell scope has been disposed.
        /// </summary>
        public static void AddDeferredTask(Func<ShellScope, Task> task) => Current?.RegisterDeferredTask(task);

        public async Task BeforeDisposeAsync()
        {
            foreach (var callback in _beforeDispose)
            {
                await callback(this);
            }

            _disposeShellContext = await TerminateShellAsync();

            var deferredTasks = _deferredTasks.ToArray();

            // Check if there are pending tasks.
            if (deferredTasks.Any())
            {
                _deferredTasks.Clear();

                // Resolve 'IShellHost' before disposing the scope which may dispose the shell.
                var shellHost = ShellContext.ServiceProvider.GetRequiredService<IShellHost>();

                // Dispose this scope.
                await DisposeAsync();

                // Then create a new scope (maybe based on a new shell) to execute tasks.
                using (var scope = await shellHost.GetScopeAsync(ShellContext.Settings))
                {
                    scope.StartAsyncFlow();

                    var factory = scope.ServiceProvider.GetService<ILoggerFactory>();
                    var logger = factory?.CreateLogger<ShellScope>();

                    for (var i = 0; i < deferredTasks.Length; i++)
                    {
                        var task = deferredTasks[i];

                        try
                        {
                            await task(scope);
                        }
                        catch (Exception e)
                        {
                            logger?.LogError(e, "Error while processing deferred task '{TaskName}' on tenant '{TenantName}'.",
                                task.GetType().FullName, ShellContext.Settings.Name);
                        }
                    }

                    await scope.BeforeDisposeAsync();
                }
            }
        }

        /// <summary>
        /// Terminate the shell, if released and in its last scope, by calling the related event handlers.
        /// Returns true if the shell context should be disposed consequently to this scope being released.
        /// </summary>
        private async Task<bool> TerminateShellAsync()
        {
            // A disabled shell still in use is released by its last scope.
            if (ShellContext.Settings.State == TenantState.Disabled)
            {
                if (Interlocked.CompareExchange(ref ShellContext._refCount, 1, 1) == 1)
                {
                    ShellContext.Release();
                }
            }

            // If the context is still being released, it will be disposed if the ref counter is equal to 0.
            // To prevent this while executing the terminating events, the ref counter is not decremented here.
            if (ShellContext._released && Interlocked.CompareExchange(ref ShellContext._refCount, 1, 1) == 1)
            {
                var tenantEvents = _serviceScope.ServiceProvider.GetServices<IModularTenantEvents>();

                foreach (var tenantEvent in tenantEvents)
                {
                    await tenantEvent.TerminatingAsync();
                }

                foreach (var tenantEvent in tenantEvents.Reverse())
                {
                    await tenantEvent.TerminatedAsync();
                }

                return true;
            }

            return false;
        }

        public async Task DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            var disposeShellContext = _disposeShellContext || await TerminateShellAsync();

            _serviceScope.Dispose();

            if (disposeShellContext)
            {
                ShellContext.Dispose();
            }

            // Decrement the counter at the very end of the scope
            Interlocked.Decrement(ref ShellContext._refCount);

            _disposed = true;
        }

        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();
    }
}
