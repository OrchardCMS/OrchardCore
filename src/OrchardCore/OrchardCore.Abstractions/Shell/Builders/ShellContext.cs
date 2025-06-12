using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell.Builders;

/// <summary>
/// Represents the state of a tenant.
/// </summary>
public class ShellContext : IDisposable, IAsyncDisposable
{
    private List<WeakReference<ShellContext>> _dependents;
    private readonly SemaphoreSlim _semaphore = new(1);
    private bool _disposed;
    private volatile int _refCount;
    private int _terminated;
    private bool _released;
    private bool _isActivating;

    /// <summary>
    /// Initializes a new <see cref="ShellContext"/>.
    /// </summary>
    public ShellContext() => UtcTicks = DateTime.UtcNow.Ticks;

    /// <summary>
    /// The creation date and time of this shell context in ticks.
    /// </summary>
    public long UtcTicks { get; }

    /// <summary>
    /// The <see cref="ShellSettings"/> holding the tenant settings and configuration.
    /// </summary>
    public ShellSettings Settings { get; set; }

    /// <summary>
    /// The <see cref="ShellBlueprint"/> describing the tenant container.
    /// </summary>
    public ShellBlueprint Blueprint { get; set; }

    /// <summary>
    /// The <see cref="IServiceProvider"/> of the tenant container.
    /// </summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    /// Whether the shell is activated or not.
    /// </summary>
    public bool IsActivated { get; private set; }

    /// <summary>
    /// The Pipeline built for this shell.
    /// </summary>
    public IShellPipeline Pipeline { get; set; }

    /// <summary>
    /// Whether or not this shell context has been disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// PlaceHolder class used for shell lazy initialization.
    /// </summary>
    public class PlaceHolder : ShellContext
    {
        /// <summary>
        /// Initializes a placeHolder used for shell lazy initialization.
        /// </summary>
        public PlaceHolder()
        {
            _released = true;
        }

        /// <summary>
        /// Whether or not the tenant has been pre-created on first loading.
        /// </summary>
        public bool PreCreated { get; init; }
    }

    /// <summary>
    /// Creates a <see cref="ShellScope"/> on this shell context.
    /// </summary>
    public async Task<ShellScope> CreateScopeAsync()
    {
        // Don't create a shell scope on a released shell.
        if (_released)
        {
            return null;
        }

        var scope = new ShellScope(this);

        // Don't start using a new scope on a released shell.
        if (_released)
        {
            // But let this scope manage the shell state as usual.
            await scope.TerminateShellAsync();
            return null;
        }

        return scope;
    }

    /// <summary>
    /// Whether the <see cref="ShellContext"/> instance is not yet built or has been released,
    /// for instance when a tenant has changed.
    /// </summary>
    public bool Released => _released;

    /// <summary>
    /// Returns the number of active scopes on this tenant.
    /// </summary>
    public int ActiveScopes => _refCount;

    /// <summary>
    /// Whether or not this instance uses shared <see cref="Settings"/> that should not be disposed.
    /// </summary>
    public bool SharedSettings { get; internal set; }

    /// <summary>
    /// Marks the <see cref="ShellContext"/> as released and then a candidate to be disposed.
    /// </summary>
    public Task ReleaseAsync() => ReleaseInternalAsync();

    private Task ReleaseFromLastScopeAsync() => ReleaseInternalAsync(ReleaseMode.FromLastScope);

    private Task ReleaseFromDependencyAsync() => ReleaseInternalAsync(ReleaseMode.FromDependency);

    private async Task ReleaseInternalAsync(ReleaseMode mode = ReleaseMode.Normal)
    {
        // A 'PlaceHolder' is always released.
        if (this is PlaceHolder)
        {
            // But still try to dispose the settings.
            await DisposeAsync();
            return;
        }

        if (_released)
        {
            // Prevent infinite loops with circular dependencies
            return;
        }

        // A disabled shell still in use will be released by its last scope, as checked at the host level.
        if (mode == ReleaseMode.FromDependency && Settings.IsDisabled() && _refCount != 0)
        {
            return;
        }

        // When a tenant has changed its shell context is replaced with a new one, so that new requests can't use it anymore.
        // However, some uncompleted requests may still try to use or resolve services from child shell scopes. In that case,
        // this is the last shell scope (when the shell reference count reaches zero) that disposes its parent shell context.

        ShellScope scope = null;
        await _semaphore.WaitAsync();
        try
        {
            if (_released)
            {
                return;
            }

            if (_dependents is not null)
            {
                foreach (var dependent in _dependents)
                {
                    if (dependent.TryGetTarget(out var shellContext))
                    {
                        await shellContext.ReleaseFromDependencyAsync();
                    }
                }
            }

            if (mode != ReleaseMode.FromLastScope && ServiceProvider is not null)
            {
                // Before marking the shell as released, we create a new scope that will manage the shell state,
                // so that we always use the same shell scope logic to check if the reference counter reached 0.
                scope = new ShellScope(this);
            }

            _released = true;
        }
        finally
        {
            _semaphore.Release();
        }

        if (mode == ReleaseMode.FromLastScope)
        {
            return;
        }

        if (scope is not null)
        {
            // Use this scope to manage the shell state as usual.
            await scope.TerminateShellAsync();
            return;
        }

        await DisposeAsync();
    }

    internal enum ReleaseMode
    {
        Normal,
        FromLastScope,
        FromDependency,
    }

    /// <summary>
    /// Registers the specified shellContext as dependent such that it is also released when the current shell context is released.
    /// </summary>
    public async Task AddDependentShellAsync(ShellContext shellContext)
    {
        // If the dependent is released, nothing to do.
        if (shellContext.Released)
        {
            return;
        }

        // If the dependency is already released.
        if (_released)
        {
            // The dependent is released immediately.
            await shellContext.ReleaseInternalAsync();
            return;
        }

        await _semaphore.WaitAsync();
        try
        {
            _dependents ??= [];

            // Remove any previous instance that represents the same tenant in case it has been released or reloaded.
            _dependents.RemoveAll(wref => !wref.TryGetTarget(out var shell) || shell.Settings.Name == shellContext.Settings.Name);

            _dependents.Add(new WeakReference<ShellContext>(shellContext));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    internal void AddRef()
    {
        // The service provider is null if we try to create
        // a scope on a disabled shell or already disposed.
        if (ServiceProvider is null)
        {
            throw new InvalidOperationException(
               $"Can't resolve a scope on tenant '{Settings.Name}' as it is disabled or disposed");
        }

        int current;
        do
        {
            current = _refCount;
            if (current < 0)
            {
                throw new InvalidOperationException(
                   $"Can't resolve a scope on tenant '{Settings.Name}' as the shell context is already terminated");
            }
            // Try to increment _refCount only if it is not <= -1
        }
        while (Interlocked.CompareExchange(ref _refCount, current + 1, current) != current);

        if (Interlocked.CompareExchange(ref _terminated, 0, 0) != 0)
        {
            // If terminated, decrement back and throw
            Interlocked.Decrement(ref _refCount);

            throw new InvalidOperationException(
               $"Can't resolve a scope on tenant '{Settings.Name}' as the shell context is already terminated");
        }
    }

    internal bool Release()
    {
        if (Interlocked.Decrement(ref _refCount) == 0)
        {
            if (Interlocked.CompareExchange(ref _terminated, 1, 1) == 1 &&
                Interlocked.CompareExchange(ref _refCount, -1, 0) == 0)
            {
                // If the shell context is terminated, we can dispose it.
                return true;
            }
        }

        return false;
    }

    internal async Task<bool> TerminateShellContextAsync()
    {
        // Check if this is the last scope that is releasing the shell context.
        if (Interlocked.CompareExchange(ref _refCount, 1, 1) != 1)
        {
            return false;
        }

        // A disabled shell still in use is released by its last scope.
        if (Settings.IsDisabled())
        {
            await ReleaseFromLastScopeAsync();
        }

        if (!_released)
        {
            return false;
        }

        // If more than one scope is still using the shell context, we can't terminate it yet.
        if (Interlocked.CompareExchange(ref _refCount, 1, 1) != 1)
        {
            return false;
        }

        // If a new last scope reached this point, ensure that the shell is terminated once.
        if (Interlocked.CompareExchange(ref _terminated, 1, 0) >= 1)
        {
            return false;
        }

        return true;
    }

    internal async Task ActivateAsync()
    {
        // Try to acquire a lock before using a new scope, so that a next process gets the last committed data.
        (var locker, var locked) = await this.TryAcquireShellActivateLockAsync();
        if (!locked)
        {
            // The retry logic increases the delay between 2 attempts (max of 10s), so if there are too
            // many concurrent requests, one may experience a timeout while waiting before a new retry.
            if (IsActivated)
            {
                // Don't throw if the shell is activated.
                return;
            }

            throw new TimeoutException($"Failed to acquire a lock before activating the tenant: {Settings.Name}");
        }

        await using var acquiredLock = locker;

        if (IsActivated || _isActivating)
        {
            // If the shell is already activated or is currently activating, do nothing.
            return;
        }

        _isActivating = true;
        try
        {
            await ShellScope.ActivateShellAsync(this);

            IsActivated = true;
        }
        finally
        {
            _isActivating = false;
        }
    }

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        GC.SuppressFinalize(this);
    }

    private void Close()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Terminate();
    }

    private async ValueTask CloseAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Terminate();
    }

    private void Terminate()
    {
        ServiceProvider = null;
        IsActivated = false;
        Blueprint = null;
        Pipeline = null;

        _semaphore?.Dispose();

        if (SharedSettings)
        {
            return;
        }

        Settings?.Dispose();
    }

    ~ShellContext() => Close();
}
