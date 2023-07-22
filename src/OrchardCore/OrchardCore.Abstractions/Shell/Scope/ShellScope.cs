using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Scope
{
    /// <summary>
    /// Custom 'IServiceScope' managing the shell state and the execution flow.
    /// </summary>
    public sealed class ShellScope : IServiceScope, IAsyncDisposable
    {
        private static readonly AsyncLocal<ShellScopeHolder> _current = new();

        private readonly AsyncServiceScope _serviceScope;
        private readonly Dictionary<object, object> _items = new();
        private readonly List<Func<ShellScope, Task>> _beforeDispose = new();
        private readonly HashSet<string> _deferredSignals = new();
        private readonly List<Func<ShellScope, Task>> _deferredTasks = new();
        private readonly List<Func<ShellScope, Exception, Task>> _exceptionHandlers = new();

        private bool _serviceScopeOnly;
        private bool _shellTerminated;
        private bool _terminated;
        private bool _disposed;

        /// <summary>
        /// Initializes a <see cref="ShellScope"/> from a given parent <see cref="Builders.ShellContext"/>.
        /// </summary>
        public ShellScope(ShellContext shellContext)
        {
            // Prevent the context from being disposed until the end of the scope.
            Interlocked.Increment(ref shellContext._refCount);
            ShellContext = shellContext;

            // The service provider is null if we try to create
            // a scope on a disabled shell or already disposed.
            if (shellContext.ServiceProvider is null)
            {
                // Keep the counter clean before failing.
                Interlocked.Decrement(ref shellContext._refCount);

                throw new InvalidOperationException(
                    $"Can't resolve a scope on tenant '{shellContext.Settings.Name}' as it is disabled or disposed");
            }

            _serviceScope = shellContext.ServiceProvider.CreateAsyncScope();
            ServiceProvider = _serviceScope.ServiceProvider;
        }

        /// <summary>
        /// The parent 'ShellContext' of this shell scope.
        /// </summary>
        public ShellContext ShellContext { get; }

        /// <summary>
        /// The 'IServiceProvider' of this shell scope.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Retrieve the parent 'ShellContext' of the current shell scope.
        /// </summary>
        public static ShellContext Context => Current?.ShellContext;

        /// <summary>
        /// Retrieve the 'IServiceProvider' of the current shell scope.
        /// </summary>
        public static IServiceProvider Services => Current?.ServiceProvider;

        /// <summary>
        /// Retrieve the current shell scope from the async flow.
        /// </summary>
        public static ShellScope Current => _current.Value?.Scope;

        /// <summary>
        /// Sets a shared item to the current shell scope.
        /// </summary>
        public static void Set(object key, object value)
        {
            if (Current is not null)
            {
                Current._items[key] = value;
            }
        }

        /// <summary>
        /// Gets a shared item from the current shell scope.
        /// </summary>
        public static object Get(object key) => Current is null ? null : Current._items.TryGetValue(key, out var value) ? value : null;

        /// <summary>
        /// Gets a shared item of a given type from the current shell scope.
        /// </summary>
        public static T Get<T>(object key) => Current is null ? default : Current._items.TryGetValue(key, out var value) ? value is T item ? item : default : default;

        /// <summary>
        /// Gets (or creates) a shared item of a given type from the current shell scope.
        /// </summary>
        public static T GetOrCreate<T>(object key, Func<T> factory)
        {
            if (Current is null)
            {
                return factory();
            }

            if (!Current._items.TryGetValue(key, out var value) || value is not T item)
            {
                Current._items[key] = item = factory();
            }

            return item;
        }

        /// <summary>
        /// Gets (or creates) a shared item of a given type from the current shell scope.
        /// </summary>
        public static T GetOrCreate<T>(object key) where T : class, new()
        {
            if (Current is null)
            {
                return new T();
            }

            if (!Current._items.TryGetValue(key, out var value) || value is not T item)
            {
                Current._items[key] = item = new T();
            }

            return item;
        }

        /// <summary>
        /// Sets a shared feature to the current shell scope.
        /// </summary>
        public static void SetFeature<T>(T value) => Set(typeof(T), value);

        /// <summary>
        /// Gets a shared feature from the current shell scope.
        /// </summary>
        public static T GetFeature<T>() => Get<T>(typeof(T));

        /// <summary>
        /// Gets (or creates) a shared feature from the current shell scope.
        /// </summary>
        public static T GetOrCreateFeature<T>(Func<T> factory) => GetOrCreate(typeof(T), factory);

        /// <summary>
        /// Gets (or creates) a shared feature from the current shell scope.
        /// </summary>
        public static T GetOrCreateFeature<T>() where T : class, new() => GetOrCreate<T>(typeof(T));

        /// <summary>
        /// Creates a child scope from the current one.
        /// </summary>
        public static Task<ShellScope> CreateChildScopeAsync()
        {
            var shellHost = Services.GetRequiredService<IShellHost>();
            return shellHost.GetScopeAsync(Context.Settings);
        }

        /// <summary>
        /// Creates a child scope from the current one.
        /// </summary>
        public static Task<ShellScope> CreateChildScopeAsync(ShellSettings settings)
        {
            var shellHost = Services.GetRequiredService<IShellHost>();
            return shellHost.GetScopeAsync(settings);
        }

        /// <summary>
        /// Creates a child scope from the current one.
        /// </summary>
        public static Task<ShellScope> CreateChildScopeAsync(string tenant)
        {
            var shellHost = Services.GetRequiredService<IShellHost>();
            return shellHost.GetScopeAsync(tenant);
        }

        /// <summary>
        /// Execute a delegate using a child scope created from the current one.
        /// </summary>
        public static async Task UsingChildScopeAsync(Func<ShellScope, Task> execute, bool activateShell = true)
        {
            await (await CreateChildScopeAsync()).UsingAsync(execute, activateShell);
        }

        /// <summary>
        /// Execute a delegate using a child scope created from the current one.
        /// </summary>
        public static async Task UsingChildScopeAsync(ShellSettings settings, Func<ShellScope, Task> execute, bool activateShell = true)
        {
            await (await CreateChildScopeAsync(settings)).UsingAsync(execute, activateShell);
        }

        /// <summary>
        /// Execute a delegate using a child scope created from the current one.
        /// </summary>
        public static async Task UsingChildScopeAsync(string tenant, Func<ShellScope, Task> execute, bool activateShell = true)
        {
            await (await CreateChildScopeAsync(tenant)).UsingAsync(execute, activateShell);
        }

        /// <summary>
        /// Start holding this shell scope along the async flow.
        /// </summary>
        public void StartAsyncFlow()
        // Use an object indirection to hold the current scope in the 'AsyncLocal',
        // so that it can be cleared in all execution contexts when it is cleared.
            => _current.Value = new ShellScopeHolder { Scope = this };

        /// <summary>
        /// Executes a delegate using this shell scope in an isolated async flow,
        /// but only as a service scope without managing the shell state and
        /// without invoking any tenant event.
        /// </summary>
        public Task UsingServiceScopeAsync(Func<ShellScope, Task> execute)
        {
            _serviceScopeOnly = true;
            return UsingAsync(execute);
        }

        /// <summary>
        /// Executes a delegate using this shell scope in an isolated async flow,
        /// while managing the shell state and invoking tenant events.
        /// </summary>
        public async Task UsingAsync(Func<ShellScope, Task> execute, bool activateShell = true)
        {
            if (Current == this)
            {
                await execute(Current);
                return;
            }

            await using (this)
            {
                StartAsyncFlow();
                try
                {
                    try
                    {
                        if (activateShell)
                        {
                            await ActivateShellInternalAsync();
                        }

                        await execute(this);
                    }
                    finally
                    {
                        await TerminateShellInternalAsync();
                    }
                }
                catch (Exception e)
                {
                    await HandleExceptionAsync(e);
                    throw;
                }
                finally
                {
                    await BeforeDisposeAsync();
                }
            }
        }

        /// <summary>
        /// Terminates a shell using this shell scope.
        /// </summary>
        internal async Task TerminateShellAsync()
        {
            await using (this)
            {
                StartAsyncFlow();
                await TerminateShellInternalAsync();
                await BeforeDisposeAsync();
            }
        }

        /// <summary>
        /// Activate the shell, if not yet done, by calling the related tenant event handlers.
        /// </summary>
        internal async Task ActivateShellInternalAsync()
        {
            if (ShellContext.IsActivated)
            {
                return;
            }

            if (_serviceScopeOnly)
            {
                return;
            }

            // Try to acquire a lock before using a new scope, so that a next process gets the last committed data.
            (var locker, var locked) = await ShellContext.TryAcquireShellActivateLockAsync();
            if (!locked)
            {
                throw new TimeoutException($"Failed to acquire a lock before activating the tenant: {ShellContext.Settings.Name}");
            }

            await using var acquiredLock = locker;

            // The tenant gets activated here.
            if (!ShellContext.IsActivated)
            {
                await new ShellScope(ShellContext).UsingAsync(async scope =>
                {
                    var tenantEvents = scope.ServiceProvider.GetServices<IModularTenantEvents>();
                    foreach (var tenantEvent in tenantEvents)
                    {
                        await tenantEvent.ActivatingAsync();
                    }

                    foreach (var tenantEvent in tenantEvents.Reverse())
                    {
                        await tenantEvent.ActivatedAsync();
                    }
                }, activateShell: false);

                ShellContext.IsActivated = true;
            }
        }

        /// <summary>
        /// Registers a delegate to be invoked when 'BeforeDisposeAsync()' is called on this scope.
        /// </summary>
        internal void BeforeDispose(Func<ShellScope, Task> callback) => _beforeDispose.Insert(0, callback);

        /// <summary>
        /// Adds a Signal (if not already present) to be sent just after 'BeforeDisposeAsync()'.
        /// </summary>
        internal void DeferredSignal(string key) => _deferredSignals.Add(key);

        /// <summary>
        /// Adds a Task to be executed in a new scope after 'BeforeDisposeAsync()'.
        /// </summary>
        internal void DeferredTask(Func<ShellScope, Task> task) => _deferredTasks.Add(task);

        /// <summary>
        /// Adds an handler to be invoked if an exception is thrown while executing in this shell scope.
        /// </summary>
        internal void ExceptionHandler(Func<ShellScope, Exception, Task> callback) => _exceptionHandlers.Add(callback);

        /// <summary>
        /// Registers a delegate to be invoked before the current shell scope will be disposed.
        /// </summary>
        public static void RegisterBeforeDispose(Func<ShellScope, Task> callback) => Current?.BeforeDispose(callback);

        /// <summary>
        /// Adds a Signal (if not already present) to be sent just before the current shell scope will be disposed.
        /// </summary>
        public static void AddDeferredSignal(string key) => Current?.DeferredSignal(key);

        /// <summary>
        /// Adds a Task to be executed in a new scope once the current shell scope has been disposed.
        /// </summary>
        public static void AddDeferredTask(Func<ShellScope, Task> task) => Current?.DeferredTask(task);

        /// <summary>
        /// Adds an handler to be invoked if an exception is thrown while executing in this shell scope.
        /// </summary>
        public static void AddExceptionHandler(Func<ShellScope, Exception, Task> handler) => Current?.ExceptionHandler(handler);

        /// <summary>
        /// Invokes the registered delegates that should be executed if an exception is thrown while executing in this shell scope.
        /// </summary>
        public async Task HandleExceptionAsync(Exception e)
        {
            foreach (var callback in _exceptionHandlers)
            {
                await callback(this, e);
            }
        }

        /// <summary>
        /// Invokes the registered delegates that should be executed before disposing this shell scope,
        /// triggers the deferred signals and executes the deferred tasks in their own isolated scope.
        /// </summary>
        internal async Task BeforeDisposeAsync()
        {
            foreach (var callback in _beforeDispose)
            {
                await callback(this);
            }

            if (_serviceScopeOnly)
            {
                return;
            }

            if (_deferredSignals.Any())
            {
                var signal = ShellContext.ServiceProvider.GetRequiredService<ISignal>();
                foreach (var key in _deferredSignals)
                {
                    await signal.SignalTokenAsync(key);
                }
            }

            if (_deferredTasks.Any())
            {
                var shellHost = ShellContext.ServiceProvider.GetRequiredService<IShellHost>();

                foreach (var task in _deferredTasks)
                {
                    // Create a new scope (maybe based on a new shell) for each task.
                    ShellScope scope;
                    try
                    {
                        // May fail if a shell was released before being disabled.
                        scope = await shellHost.GetScopeAsync(ShellContext.Settings);
                    }
                    catch
                    {
                        // Fallback to a scope based on the current shell that is not yet disposed.
                        scope = new ShellScope(ShellContext);
                    }

                    // Use 'UsingAsync' in place of 'UsingServiceScopeAsync()' to allow a deferred task to
                    // trigger another one, but still prevent the shell to be activated in a deferred task.
                    await scope.UsingAsync(async scope =>
                    {
                        var logger = scope.ServiceProvider.GetService<ILogger<ShellScope>>();

                        try
                        {
                            await task(scope);
                        }
                        catch (Exception e)
                        {
                            logger?.LogError(e,
                                "Error while processing deferred task '{TaskName}' on tenant '{TenantName}'.",
                                task.GetType().FullName, ShellContext.Settings.Name);

                            await scope.HandleExceptionAsync(e);
                        }
                    },
                    activateShell: false);
                }
            }
        }

        /// <summary>
        /// Terminates the shell, if released and in its last scope, by calling the related event handlers,
        /// and specifies if the shell context should be disposed consequently to this scope being disposed.
        /// </summary>
        internal async Task TerminateShellInternalAsync()
        {
            if (_serviceScopeOnly)
            {
                return;
            }

            _terminated = true;

            // If the shell context is released and in its last shell scope, according to the ref counter value,
            // the terminate event handlers are called, and the shell will be disposed at the end of this scope.

            // Check if the decremented value of the ref counter reached 0.
            if (Interlocked.Decrement(ref ShellContext._refCount) == 0)
            {
                // A disabled shell still in use is released by its last scope.
                if (ShellContext.Settings.IsDisabled())
                {
                    await ShellContext.ReleaseFromLastScopeAsync();
                }

                if (!ShellContext._released)
                {
                    return;
                }

                // If released after the counter reached 0, a new last scope may have been created.
                if (Interlocked.CompareExchange(ref ShellContext._refCount, 0, 0) != 0)
                {
                    return;
                }

                // If a new last scope reached this point, ensure that the shell is terminated once.
                if (Interlocked.Exchange(ref ShellContext._terminated, 1) == 1)
                {
                    return;
                }

                _shellTerminated = true;

                var tenantEvents = _serviceScope.ServiceProvider.GetServices<IModularTenantEvents>();
                foreach (var tenantEvent in tenantEvents)
                {
                    await tenantEvent.TerminatingAsync();
                }

                foreach (var tenantEvent in tenantEvents.Reverse())
                {
                    await tenantEvent.TerminatedAsync();
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _serviceScope.Dispose();

            if (_shellTerminated)
            {
                ShellContext.Dispose();
            }

            Terminate();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            await _serviceScope.DisposeAsync();

            if (_shellTerminated)
            {
                await ShellContext.DisposeAsync();
            }

            Terminate();
        }

        private void Terminate()
        {
            if (!_terminated)
            {
                // Keep the counter clean if not yet decremented.
                Interlocked.Decrement(ref ShellContext._refCount);
            }

            var holder = _current.Value;
            if (holder is not null)
            {
                // Clear the current scope that may be trapped in some execution contexts.
                holder.Scope = null;
            }
        }

        private class ShellScopeHolder
        {
            public ShellScope Scope;
        }
    }
}
