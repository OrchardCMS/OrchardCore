using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Distributed;

/// <summary>
/// Keep in sync tenants by sharing shell identifiers through an <see cref="IDistributedCache"/>.
/// </summary>
internal sealed class DistributedShellHostedService : BackgroundService
{
    private const string DistributedFeatureId = "OrchardCore.Tenants.Distributed";

    private const string ShellChangedIdKey = "SHELL_CHANGED_ID";
    private const string ShellCountChangedIdKey = "SHELL_COUNT_CHANGED_ID";
    private const string ReleaseIdKeySuffix = "_RELEASE_ID";
    private const string ReloadIdKeySuffix = "_RELOAD_ID";

    private static readonly TimeSpan _minIdleTime = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan _maxRetryTime = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan _maxBusyTime = TimeSpan.FromSeconds(2);

    private readonly IShellHost _shellHost;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly IShellSettingsManager _shellSettingsManager;
    private readonly IShellRemovalManager _shellRemovingManager;
    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<string, ShellIdentifier> _identifiers = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    private string _shellChangedId;
    private string _shellCountChangedId;

    private long _defaultContextUtcTicks;
    private DistributedContext _context;

    private DateTime _busyStartTime;
    private bool _terminated;

    public DistributedShellHostedService(
        IShellHost shellHost,
        IShellContextFactory shellContextFactory,
        IShellSettingsManager shellSettingsManager,
        IShellRemovalManager shellRemovingManager,
        ILogger<DistributedShellHostedService> logger)
    {
        _shellHost = shellHost;
        _shellContextFactory = shellContextFactory;
        _shellSettingsManager = shellSettingsManager;
        _shellRemovingManager = shellRemovingManager;
        _logger = logger;

        shellHost.LoadingAsync += LoadingAsync;
        shellHost.ReleasingAsync += ReleasingAsync;
        shellHost.ReloadingAsync += ReloadingAsync;
        shellHost.RemovingAsync += RemovingAsync;
    }

    /// <summary>
    /// Keep in sync tenants by sharing shell identifiers through an <see cref="IDistributedCache"/>.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The syncing period in seconds of the default tenant while it is 'Uninitialized'.
        const int DefaultTenantSyncingPeriod = 20;

        stoppingToken.Register(() =>
        {
            _logger.LogInformation("'{ServiceName}' is stopping.", nameof(DistributedShellHostedService));
        });

        // Init the idle time.
        var idleTime = _minIdleTime;

        // Init the second counter used to sync the default tenant while it is 'Uninitialized'.
        var defaultTenantSyncingSeconds = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for the current idle time on each loop.
                if (!await TryWaitAsync(idleTime, stoppingToken))
                {
                    break;
                }

                // If there is no default tenant, nothing to do.
                if (!_shellHost.TryGetShellContext(ShellSettings.DefaultShellName, out var defaultContext))
                {
                    continue;
                }

                // Manage the second counter used to sync the default tenant while it is 'Uninitialized'.
                defaultTenantSyncingSeconds = defaultContext.Settings.IsUninitialized()
                    ? defaultTenantSyncingSeconds
                    : 0;

                // Check periodically if the default tenant is still 'Uninitialized'.
                if (defaultTenantSyncingSeconds++ > DefaultTenantSyncingPeriod)
                {
                    defaultTenantSyncingSeconds = 0;

                    // Load the settings of the default tenant that may have been setup by another instance.
                    using var loadedDefaultSettings = (await _shellSettingsManager
                        .LoadSettingsAsync(ShellSettings.DefaultShellName))
                        .AsDisposable();

                    if (loadedDefaultSettings.IsRunning())
                    {
                        // If the default tenant has been setup by another instance, reload it locally.
                        await _shellHost.ReloadShellContextAsync(defaultContext.Settings, eventSource: false);
                    }

                    continue;
                }

                // If the default tenant is not running, nothing to do.
                if (!defaultContext.Settings.IsRunning())
                {
                    continue;
                }

                // Get or create a new distributed context if the default tenant has changed.
                var context = await GetOrCreateDistributedContextAsync(defaultContext);

                // If the required distributed features are not enabled, nothing to do.
                var distributedCache = context?.DistributedCache;
                if (distributedCache is null)
                {
                    continue;
                }

                // Try to retrieve the tenant changed global identifier from the distributed cache.
                string shellChangedId;
                try
                {
                    shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey, CancellationToken.None);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    // Get the next idle time before retrying to read the distributed cache.
                    idleTime = NextIdleTimeBeforeRetry(idleTime, ex);
                    continue;
                }

                // Reset the idle time.
                idleTime = _minIdleTime;

                // Check if at least one tenant has changed.
                if (shellChangedId is null || _shellChangedId == shellChangedId)
                {
                    continue;
                }

                // Try to retrieve the tenant list changed global identifier from the distributed cache.
                string shellCountChangedId;
                try
                {
                    shellCountChangedId = await distributedCache.GetStringAsync(ShellCountChangedIdKey, CancellationToken.None);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.LogError(ex, "Unable to read the distributed cache before checking if a tenant has been created or removed.");
                    continue;
                }

                // Retrieve all tenant settings that are loaded locally.
                var loadedSettings = _shellHost.GetAllSettings().ToList();
                var tenantsToRemove = Array.Empty<string>();
                var tenantsToCreate = Array.Empty<string>();

                // Check if at least one tenant has been created or removed.
                if (shellCountChangedId is not null && _shellCountChangedId != shellCountChangedId)
                {
                    var sharedTenants = await _shellSettingsManager.LoadSettingsNamesAsync();
                    var loadedTenants = loadedSettings.Select(s => s.Name);

                    // Retrieve all new created tenants that are not already loaded.
                    tenantsToCreate = sharedTenants.Except(loadedTenants).ToArray();

                    // Load all new created tenants.
                    foreach (var tenant in tenantsToCreate)
                    {
                        loadedSettings.Add((await _shellSettingsManager
                            .LoadSettingsAsync(tenant))
                            .AsDisposable());
                    }

                    // Retrieve all removed tenants that are not yet removed locally.
                    tenantsToRemove = loadedTenants.Except(sharedTenants).ToArray();
                }

                // Init the busy start time.
                var _busyStartTime = DateTime.UtcNow;
                var syncingSuccess = true;

                // Keep in sync all tenants by checking their specific identifiers.
                foreach (var settings in loadedSettings)
                {
                    // Newly loaded settings from the configuration should be disposed.
                    using var disposable = tenantsToCreate.Contains(settings.Name) ? settings : null;

                    // Wait for the min idle time after the max busy time.
                    if (!await TryWaitAfterBusyTime(stoppingToken))
                    {
                        break;
                    }

                    var semaphore = _semaphores.GetOrAdd(settings.Name, name => new SemaphoreSlim(1));
                    await semaphore.WaitAsync(CancellationToken.None);
                    try
                    {
                        // Try to retrieve the release identifier of this tenant from the distributed cache.
                        var releaseId = await distributedCache.GetStringAsync(ReleaseIdKey(settings.Name), CancellationToken.None);
                        if (releaseId is not null && !tenantsToCreate.Contains(settings.Name))
                        {
                            // Check if the release identifier of this tenant has changed.
                            var identifier = _identifiers.GetOrAdd(settings.Name, name => new ShellIdentifier());
                            if (identifier.ReleaseId != releaseId)
                            {
                                // Update the local identifier.
                                identifier.ReleaseId = releaseId;

                                // Keep in sync this tenant by releasing it locally.
                                await _shellHost.ReleaseShellContextAsync(settings, eventSource: false);
                            }
                        }

                        // Try to retrieve the reload identifier of this tenant from the distributed cache.
                        var reloadId = await distributedCache.GetStringAsync(ReloadIdKey(settings.Name), CancellationToken.None);
                        if (reloadId is not null)
                        {
                            // Check if the reload identifier of this tenant has changed.
                            var identifier = _identifiers.GetOrAdd(settings.Name, name => new ShellIdentifier());
                            if (identifier.ReloadId != reloadId)
                            {
                                // Update the local identifier.
                                identifier.ReloadId = reloadId;

                                // For a new tenant also update the release identifier.
                                if (tenantsToCreate.Contains(settings.Name))
                                {
                                    identifier.ReleaseId = releaseId;
                                }

                                // Keep in sync this tenant by reloading it locally.
                                await _shellHost.ReloadShellContextAsync(settings, eventSource: false);
                            }
                        }

                        // Check if the tenant needs to be removed locally.
                        if (!settings.IsDefaultShell() && tenantsToRemove.Contains(settings.Name))
                        {
                            // The local resources can only be removed if the tenant is 'Disabled' or 'Uninitialized'.
                            if (settings.IsRemovable())
                            {
                                // Keep in sync this tenant by removing its local (non shared) resources.
                                var removingContext = await _shellRemovingManager.RemoveAsync(settings, localResourcesOnly: true);
                                if (removingContext.FailedOnLockTimeout)
                                {
                                    // If it only failed to acquire a lock, let it retry on the next loop.
                                    syncingSuccess = false;
                                }
                                else
                                {
                                    // Otherwise, keep in sync this tenant by removing the shell locally.
                                    await _shellHost.RemoveShellContextAsync(settings, eventSource: false);

                                    // Cleanup local dictionaries.
                                    _identifiers.TryRemove(settings.Name, out _);
                                    _semaphores.TryRemove(settings.Name, out _);
                                }
                            }
                        }
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        syncingSuccess = false;
                        _logger.LogError(ex, "Unexpected error while syncing the tenant '{TenantName}'.", settings.Name);
                        break;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }

                // Keep in sync the tenant global identifiers.
                if (syncingSuccess)
                {
                    _shellChangedId = shellChangedId;
                    _shellCountChangedId = shellCountChangedId;
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Error while executing '{ServiceName}'", nameof(DistributedShellHostedService));
            }
        }

        _terminated = true;

        _shellHost.LoadingAsync -= LoadingAsync;
        _shellHost.ReleasingAsync -= ReleasingAsync;
        _shellHost.ReloadingAsync -= ReloadingAsync;
        _shellHost.RemovingAsync -= RemovingAsync;

        if (_context is not null)
        {
            await _context.ReleaseAsync();
        }

        _context = null;

        foreach (var semaphore in _semaphores.Values)
        {
            semaphore.Dispose();
        }

        _semaphores.Clear();
    }

    /// <summary>
    /// Called before loading all tenants to initialize the local shell identifiers from the distributed cache.
    /// </summary>
    public async Task LoadingAsync()
    {
        if (_terminated)
        {
            return;
        }

        // Load a first isolated configuration before the default context is initialized.
        var defaultSettings = (await _shellSettingsManager
            .LoadSettingsAsync(ShellSettings.DefaultShellName))
            .AsDisposable();

        // If there is no default tenant or it is not running, nothing to do.
        if (!defaultSettings.IsRunning())
        {
            defaultSettings.Dispose();
            return;
        }

        // Create a distributed context based on the first loaded isolated configuration.
        var context = _context = (await CreateDistributedContextAsync(defaultSettings))
            ?.WithoutSharedSettings();

        if (context is null)
        {
            defaultSettings.Dispose();
            return;
        }

        // If the required distributed features are not enabled, nothing to do.
        var distributedCache = context.DistributedCache;
        if (distributedCache is null)
        {
            return;
        }

        try
        {
            // Retrieve the tenant global identifiers from the distributed cache.
            var shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey);
            var shellCountChangedId = await distributedCache.GetStringAsync(ShellCountChangedIdKey);

            // Retrieve the names of all the tenants.
            var names = await _shellSettingsManager.LoadSettingsNamesAsync();
            foreach (var name in names)
            {
                // Retrieve the release identifier of this tenant from the distributed cache.
                var releaseId = await distributedCache.GetStringAsync(ReleaseIdKey(name));
                if (releaseId is not null)
                {
                    // Initialize the release identifier of this tenant in the local collection.
                    var identifier = _identifiers.GetOrAdd(name, name => new ShellIdentifier());
                    identifier.ReleaseId = releaseId;
                }

                // Retrieve the reload identifier of this tenant from the distributed cache.
                var reloadId = await distributedCache.GetStringAsync(ReloadIdKey(name));
                if (reloadId is not null)
                {
                    // Initialize the reload identifier of this tenant in the local collection.
                    var identifier = _identifiers.GetOrAdd(name, name => new ShellIdentifier());
                    identifier.ReloadId = reloadId;
                }
            }

            // Keep in sync the tenant global identifiers.
            _shellChangedId = shellChangedId;
            _shellCountChangedId = shellCountChangedId;
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _logger.LogError(ex, "Unable to read the distributed cache before loading all tenants.");
        }
    }

    /// <summary>
    /// Called before releasing a tenant to update the related shell identifiers, locally and in the distributed cache.
    /// </summary>
    public async Task ReleasingAsync(string name)
    {
        if (_terminated)
        {
            return;
        }

        // If there is no default tenant or it is not running, nothing to do.
        if (!_shellHost.TryGetShellContext(ShellSettings.DefaultShellName, out var defaultContext) ||
            !defaultContext.Settings.IsRunning())
        {
            return;
        }

        // Acquire the distributed context or create a new one if not yet built.
        await using var context = await AcquireOrCreateDistributedContextAsync(defaultContext);

        // If the required distributed features are not enabled, nothing to do.
        var distributedCache = context?.DistributedCache;
        if (distributedCache is null)
        {
            return;
        }

        var semaphore = _semaphores.GetOrAdd(name, name => new SemaphoreSlim(1));
        await semaphore.WaitAsync();
        try
        {
            // Update this tenant in the local collection with a new release identifier.
            var identifier = _identifiers.GetOrAdd(name, name => new ShellIdentifier());
            identifier.ReleaseId = IdGenerator.GenerateId();

            // Update the release identifier of this tenant in the distributed cache.
            await distributedCache.SetStringAsync(ReleaseIdKey(name), identifier.ReleaseId);

            // Also update the global identifier specifying that a tenant has changed.
            await distributedCache.SetStringAsync(ShellChangedIdKey, identifier.ReleaseId);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _logger.LogError(ex, "Unable to update the distributed cache before releasing the tenant '{TenantName}'.", name);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Called before reloading a tenant to update the related shell identifiers, locally and in the distributed cache.
    /// </summary>
    public async Task ReloadingAsync(string name)
    {
        if (_terminated)
        {
            return;
        }

        // If there is no default tenant or it is not running, nothing to do.
        if (!_shellHost.TryGetShellContext(ShellSettings.DefaultShellName, out var defaultContext) ||
            !defaultContext.Settings.IsRunning())
        {
            return;
        }

        // Acquire the distributed context or create a new one if not yet built.
        await using var context = await AcquireOrCreateDistributedContextAsync(defaultContext);

        // If the context still uses the first isolated configuration.
        if (context is not null && !context.Context.SharedSettings)
        {
            // Reset the serial number so that a new context will be built.
            context.Context.Blueprint.Descriptor.SerialNumber = 0;
        }

        // If the required distributed features are not enabled, nothing to do.
        var distributedCache = context?.DistributedCache;
        if (distributedCache is null)
        {
            return;
        }

        var semaphore = _semaphores.GetOrAdd(name, name => new SemaphoreSlim(1));
        await semaphore.WaitAsync();
        try
        {
            // Update this tenant in the local collection with a new reload identifier.
            var identifier = _identifiers.GetOrAdd(name, name => new ShellIdentifier());
            identifier.ReloadId = IdGenerator.GenerateId();

            // Update the reload identifier of this tenant in the distributed cache.
            await distributedCache.SetStringAsync(ReloadIdKey(name), identifier.ReloadId);

            // Check if it is a new created tenant that has not been already loaded.
            if (!name.IsDefaultShellName() && !_shellHost.TryGetSettings(name, out _))
            {
                // Also update the global identifier specifying that a tenant has been created.
                await distributedCache.SetStringAsync(ShellCountChangedIdKey, identifier.ReloadId);
            }

            // Also update the global identifier specifying that a tenant has changed.
            await distributedCache.SetStringAsync(ShellChangedIdKey, identifier.ReloadId);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _logger.LogError(ex, "Unable to update the distributed cache before reloading the tenant '{TenantName}'.", name);
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Called before removing a tenant to update the related shell identifiers, locally and in the distributed cache.
    /// </summary>
    public async Task RemovingAsync(string name)
    {
        // The 'Default' tenant can't be removed.
        if (_terminated || name.IsDefaultShellName())
        {
            return;
        }

        // If there is no default tenant or it is not running, nothing to do.
        if (!_shellHost.TryGetShellContext(ShellSettings.DefaultShellName, out var defaultContext) ||
            !defaultContext.Settings.IsRunning())
        {
            return;
        }

        // Acquire the distributed context or create a new one if not yet built.
        await using var context = await AcquireOrCreateDistributedContextAsync(defaultContext);

        // If the required distributed features are not enabled, nothing to do.
        var distributedCache = context?.DistributedCache;
        if (distributedCache is null)
        {
            return;
        }

        var semaphore = _semaphores.GetOrAdd(name, name => new SemaphoreSlim(1));
        await semaphore.WaitAsync();
        try
        {
            var removedId = IdGenerator.GenerateId();

            // Also update the global identifier specifying that a tenant has been removed.
            await distributedCache.SetStringAsync(ShellCountChangedIdKey, removedId);

            // Also update the global identifier specifying that a tenant has changed.
            await distributedCache.SetStringAsync(ShellChangedIdKey, removedId);
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _logger.LogError(ex, "Unable to update the distributed cache before removing the tenant '{TenantName}'.", name);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static string ReleaseIdKey(string name) => $"{name}{ReleaseIdKeySuffix}";
    private static string ReloadIdKey(string name) => $"{name}{ReloadIdKeySuffix}";

    /// <summary>
    /// Gets or creates a new distributed context if the default tenant has changed.
    /// </summary>
    private async Task<DistributedContext> GetOrCreateDistributedContextAsync(ShellContext defaultContext)
    {
        // Check if the default tenant has changed.
        if (_defaultContextUtcTicks != defaultContext.UtcTicks)
        {
            var previousContext = _context;

            // Reuse or create a new context based on the default tenant.
            _context = await ReuseOrCreateDistributedContextAsync(defaultContext);

            // Cache the default context.
            _defaultContextUtcTicks = defaultContext.UtcTicks;

            // If the context is not reused.
            if (_context != previousContext && previousContext is not null)
            {
                // Release the previous one.
                await previousContext.ReleaseAsync();
            }
        }

        return _context;
    }

    /// <summary>
    /// Reuses or creates a new distributed context based on the default tenant context.
    /// </summary>
    private async Task<DistributedContext> ReuseOrCreateDistributedContextAsync(ShellContext defaultContext)
    {
        // If no context.
        if (_context is null)
        {
            // Create a new context based on the default context.
            return await CreateDistributedContextAsync(defaultContext);
        }

        // Check if the default context is still the placeholder pre-created on first loading.
        if (defaultContext.IsPreCreated())
        {
            // Reuse the current context.
            return _context;
        }

        // Get the default tenant descriptor.
        var descriptor = await GetDefaultShellDescriptorAsync(defaultContext);

        // If no descriptor.
        if (descriptor is null)
        {
            // Nothing to create.
            return null;
        }

        // Check if the default tenant descriptor or tenant configuration was updated.
        if (_context.Context.Blueprint.Descriptor.SerialNumber != descriptor.SerialNumber ||
            !_context.Context.Settings.HasConfiguration())
        {
            // Creates a new context based on the default settings and descriptor.
            return await CreateDistributedContextAsync(defaultContext.Settings, descriptor);
        }

        // Reuse the current context.
        return _context;
    }

    /// <summary>
    /// Acquires the distributed context or creates a new one if not yet initialized.
    /// </summary>
    private Task<DistributedContext> AcquireOrCreateDistributedContextAsync(ShellContext defaultContext)
    {
        // Acquire the current context.
        var distributedContext = _context?.Acquire();
        if (distributedContext is null)
        {
            // Create a new context based on the default context.
            return CreateDistributedContextAsync(defaultContext);
        }

        return Task.FromResult(distributedContext);
    }

    /// <summary>
    /// Creates a distributed context based on the default tenant context.
    /// </summary>
    private async Task<DistributedContext> CreateDistributedContextAsync(ShellContext defaultContext)
    {
        // Get the default tenant descriptor.
        var descriptor = await GetDefaultShellDescriptorAsync(defaultContext);

        // If no descriptor.
        if (descriptor is null)
        {
            // Nothing to create.
            return null;
        }

        // Creates a new context based on the default settings and descriptor.
        return await CreateDistributedContextAsync(defaultContext.Settings, descriptor);
    }

    /// <summary>
    /// Creates a distributed context based on the default tenant settings.
    /// </summary>
    private async Task<DistributedContext> CreateDistributedContextAsync(ShellSettings defaultSettings)
    {
        // Get the default tenant descriptor.
        var descriptor = await GetDefaultShellDescriptorAsync(defaultSettings);

        // If no descriptor.
        if (descriptor is null)
        {
            // Nothing to create.
            return null;
        }

        // Creates a new context based on the default settings and descriptor.
        return await CreateDistributedContextAsync(defaultSettings, descriptor);
    }

    /// <summary>
    /// Creates a distributed context based on the default tenant settings and descriptor.
    /// </summary>
    private async Task<DistributedContext> CreateDistributedContextAsync(ShellSettings defaultSettings, ShellDescriptor descriptor)
    {
        // Using the current shell descriptor prevents a database access, and a race condition
        // when resolving `IStore` while the default tenant is activating and does migrations.
        try
        {
            return new DistributedContext(await _shellContextFactory.CreateDescribedContextAsync(defaultSettings, descriptor));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the default tenant descriptor based on the default tenant context.
    /// </summary>
    private Task<ShellDescriptor> GetDefaultShellDescriptorAsync(ShellContext defaultContext)
    {
        // Check if the configuration has been disposed.
        if (!defaultContext.Settings.HasConfiguration())
        {
            return Task.FromResult<ShellDescriptor>(null);
        }

        // Capture the descriptor as the blueprint may be set to null right after.
        var descriptor = defaultContext.Blueprint?.Descriptor;

        // Check if the distributed feature is enabled.
        if (descriptor?.Features.Any(feature => feature.Id == DistributedFeatureId) ?? false)
        {
            return Task.FromResult(descriptor);
        }

        // No descriptor if the default context is a placeholder without blueprint.
        if (descriptor is null)
        {
            // Get the default tenant descriptor from the store.
            return GetDefaultShellDescriptorAsync(defaultContext.Settings);
        }

        return Task.FromResult<ShellDescriptor>(null);
    }

    /// <summary>
    /// Gets the default tenant descriptor from the store based on the default tenant configuration.
    /// </summary>
    private async Task<ShellDescriptor> GetDefaultShellDescriptorAsync(ShellSettings defaultSettings)
    {
        // Check if the configuration has been disposed.
        if (!defaultSettings.HasConfiguration())
        {
            return null;
        }

        try
        {
            // Get the descriptor from the store and check if the distributed feature is enabled.
            var descriptor = await _shellContextFactory.GetShellDescriptorAsync(defaultSettings);
            if (descriptor?.Features.Any(feature => feature.Id == DistributedFeatureId) ?? false)
            {
                return descriptor;
            }
        }
        catch
        {
        }

        return null;
    }

    /// <summary>
    /// Gets the next idle time before retrying to read the distributed cache.
    /// </summary>
    private TimeSpan NextIdleTimeBeforeRetry(TimeSpan idleTime, Exception ex)
    {
        if (idleTime < _maxRetryTime)
        {
            // Log an error on each retry, but only before reaching the 'MaxRetryTime', to not fill out the log.
            _logger.LogError(ex, "Unable to read the distributed cache before checking if a tenant has changed.");

            idleTime *= 2;
            if (idleTime > _maxRetryTime)
            {
                idleTime = _maxRetryTime;
            }
        }

        return idleTime;
    }

    /// <summary>
    /// Tries to wait for the min idle time after the max busy time, returns false if it was cancelled.
    /// </summary>
    private async Task<bool> TryWaitAfterBusyTime(CancellationToken stoppingToken)
    {
        if (DateTime.UtcNow - _busyStartTime > _maxBusyTime)
        {
            if (!await TryWaitAsync(_minIdleTime, stoppingToken))
            {
                return false;
            }

            _busyStartTime = DateTime.UtcNow;
        }

        return true;
    }

    /// <summary>
    /// Tries to wait for a given delay, returns false if it was cancelled.
    /// </summary>
    private static async Task<bool> TryWaitAsync(TimeSpan delay, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(delay, stoppingToken);
            return true;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }
}
