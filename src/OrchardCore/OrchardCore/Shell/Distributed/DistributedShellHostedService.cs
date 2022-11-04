using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Distributed
{
    /// <summary>
    /// Keep in sync tenants by sharing shell identifiers through an <see cref="IDistributedCache"/>.
    /// </summary>
    internal class DistributedShellHostedService : BackgroundService
    {
        private const string ShellChangedIdKey = "SHELL_CHANGED_ID";
        private const string ShellCreatedIdKey = "SHELL_CREATED_ID";
        private const string ReleaseIdKeySuffix = "_RELEASE_ID";
        private const string ReloadIdKeySuffix = "_RELOAD_ID";

        private static readonly TimeSpan MinIdleTime = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan MaxRetryTime = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan MaxBusyTime = TimeSpan.FromSeconds(2);

        private readonly IShellHost _shellHost;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, ShellIdentifier> _identifiers = new ConcurrentDictionary<string, ShellIdentifier>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        private string _shellChangedId;
        private string _shellCreatedId;

        private ShellContext _defaultContext;
        private DistributedContext _context;

        private DateTime _busyStartTime;
        private bool _terminated;

        public DistributedShellHostedService(
            IShellHost shellHost,
            IShellContextFactory shellContextFactory,
            IShellSettingsManager shellSettingsManager,
            ILogger<DistributedShellHostedService> logger)
        {
            _shellHost = shellHost;
            _shellContextFactory = shellContextFactory;
            _shellSettingsManager = shellSettingsManager;
            _logger = logger;

            shellHost.LoadingAsync += LoadingAsync;
            shellHost.ReleasingAsync += ReleasingAsync;
            shellHost.ReloadingAsync += ReloadingAsync;
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
            var idleTime = MinIdleTime;

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
                    if (!_shellHost.TryGetShellContext(ShellHelper.DefaultShellName, out var defaultContext))
                    {
                        continue;
                    }

                    // Manage the second counter used to sync the default tenant while it is 'Uninitialized'.
                    defaultTenantSyncingSeconds = defaultContext.Settings.State == TenantState.Uninitialized
                        ? defaultTenantSyncingSeconds
                        : 0;

                    // Check periodically if the default tenant is still 'Uninitialized'.
                    if (defaultTenantSyncingSeconds++ > DefaultTenantSyncingPeriod)
                    {
                        defaultTenantSyncingSeconds = 0;

                        // Load the settings of the default tenant that may have been setup by another instance.
                        var defaultSettings = await _shellSettingsManager.LoadSettingsAsync(ShellHelper.DefaultShellName);
                        if (defaultSettings.State == TenantState.Running)
                        {
                            // If the default tenant has been setup by another instance, reload it locally.
                            await _shellHost.ReloadShellContextAsync(defaultContext.Settings, eventSource: false);
                        }

                        continue;
                    }

                    // If the default tenant is not running, nothing to do.
                    if (defaultContext.Settings.State != TenantState.Running)
                    {
                        continue;
                    }

                    // Get or create a new distributed context if the default tenant has changed.
                    var context = await GetOrCreateDistributedContextAsync(defaultContext);

                    // If the required distributed features are not enabled, nothing to do.
                    var distributedCache = context?.DistributedCache;
                    if (distributedCache == null)
                    {
                        continue;
                    }

                    // Try to retrieve the tenant changed global identifier from the distributed cache.
                    string shellChangedId;
                    try
                    {
                        shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        // Get the next idle time before retrying to read the distributed cache.
                        idleTime = NextIdleTimeBeforeRetry(idleTime, ex);
                        continue;
                    }

                    // Reset the idle time.
                    idleTime = MinIdleTime;

                    // Check if at least one tenant has changed.
                    if (shellChangedId == null || _shellChangedId == shellChangedId)
                    {
                        continue;
                    }

                    // Try to retrieve the tenant created global identifier from the distributed cache.
                    string shellCreatedId;
                    try
                    {
                        shellCreatedId = await distributedCache.GetStringAsync(ShellCreatedIdKey);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        _logger.LogError(ex, "Unable to read the distributed cache before checking if a tenant has been created.");
                        continue;
                    }

                    // Retrieve all tenant settings that are already loaded.
                    var allSettings = _shellHost.GetAllSettings().ToList();

                    // Check if at least one tenant has been created.
                    if (shellCreatedId != null && _shellCreatedId != shellCreatedId)
                    {
                        // Retrieve all new created tenants that are not already loaded.
                        var names = (await _shellSettingsManager.LoadSettingsNamesAsync())
                            .Except(allSettings.Select(s => s.Name))
                            .ToArray();

                        // Load and enlist the settings of all new created tenant.
                        foreach (var name in names)
                        {
                            allSettings.Add(await _shellSettingsManager.LoadSettingsAsync(name));
                        }
                    }

                    // Init the busy start time.
                    var _busyStartTime = DateTime.UtcNow;
                    var syncingSuccess = true;

                    // Keep in sync all tenants by checking their specific identifiers.
                    foreach (var settings in allSettings)
                    {
                        // Wait for the min idle time after the max busy time.
                        if (!await TryWaitAfterBusyTime(stoppingToken))
                        {
                            break;
                        }

                        var semaphore = _semaphores.GetOrAdd(settings.Name, name => new SemaphoreSlim(1));
                        await semaphore.WaitAsync();
                        try
                        {
                            // Try to retrieve the release identifier of this tenant from the distributed cache.
                            var releaseId = await distributedCache.GetStringAsync(ReleaseIdKey(settings.Name));
                            if (releaseId != null)
                            {
                                // Check if the release identifier of this tenant has changed.
                                var identifier = _identifiers.GetOrAdd(settings.Name, name => new ShellIdentifier());
                                if (identifier.ReleaseId != releaseId)
                                {
                                    // Upate the local identifier.
                                    identifier.ReleaseId = releaseId;

                                    // Keep in sync this tenant by releasing it locally.
                                    await _shellHost.ReleaseShellContextAsync(settings, eventSource: false);
                                }
                            }

                            // Try to retrieve the reload identifier of this tenant from the distributed cache.
                            var reloadId = await distributedCache.GetStringAsync(ReloadIdKey(settings.Name));
                            if (reloadId != null)
                            {
                                // Check if the reload identifier of this tenant has changed.
                                var identifier = _identifiers.GetOrAdd(settings.Name, name => new ShellIdentifier());
                                if (identifier.ReloadId != reloadId)
                                {
                                    // Upate the local identifier.
                                    identifier.ReloadId = reloadId;

                                    // Keep in sync this tenant by reloading it locally.
                                    await _shellHost.ReloadShellContextAsync(settings, eventSource: false);
                                }
                            }
                        }
                        catch (Exception ex) when (!ex.IsFatal())
                        {
                            syncingSuccess = false;
                            _logger.LogError(ex, "Unable to read the distributed cache while syncing the tenant '{TenantName}'.", settings.Name);
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
                        _shellCreatedId = shellCreatedId;
                    }
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.LogError(ex, "Error while executing '{ServiceName}'", nameof(DistributedShellHostedService));
                }
            }

            _terminated = true;
            _context?.Release();
            _defaultContext = null;
            _context = null;
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

            // If there is no default tenant or it is not running, nothing to do.
            var defautSettings = await _shellSettingsManager.LoadSettingsAsync(ShellHelper.DefaultShellName);
            if (defautSettings?.State != TenantState.Running)
            {
                return;
            }

            // Create a local distributed context because it is not yet initialized.
            using var context = await CreateDistributedContextAsync(defautSettings);

            // If the required distributed features are not enabled, nothing to do.
            var distributedCache = context?.DistributedCache;
            if (distributedCache == null)
            {
                return;
            }

            try
            {
                // Retrieve the tenant global identifiers from the distributed cache.
                var shellChangedId = await distributedCache.GetStringAsync(ShellChangedIdKey);
                var shellCreatedId = await distributedCache.GetStringAsync(ShellCreatedIdKey);

                // Retrieve the names of all the tenants.
                var names = await _shellSettingsManager.LoadSettingsNamesAsync();
                foreach (var name in names)
                {
                    // Retrieve the release identifier of this tenant from the distributed cache.
                    var releaseId = await distributedCache.GetStringAsync(ReleaseIdKey(name));
                    if (releaseId != null)
                    {
                        // Initialize the release identifier of this tenant in the local collection.
                        var identifier = _identifiers.GetOrAdd(name, name => new ShellIdentifier());
                        identifier.ReleaseId = releaseId;
                    }

                    // Retrieve the reload identifier of this tenant from the distributed cache.
                    var reloadId = await distributedCache.GetStringAsync(ReloadIdKey(name));
                    if (reloadId != null)
                    {
                        // Initialize the reload identifier of this tenant in the local collection.
                        var identifier = _identifiers.GetOrAdd(name, name => new ShellIdentifier());
                        identifier.ReloadId = reloadId;
                    }
                }

                // Keep in sync the tenant global identifiers.
                _shellChangedId = shellChangedId;
                _shellCreatedId = shellCreatedId;
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
            if (!_shellHost.TryGetShellContext(ShellHelper.DefaultShellName, out var defaultContext) ||
                defaultContext.Settings.State != TenantState.Running)
            {
                return;
            }

            // Acquire the distributed context or create a new one if not yet built.
            using var context = await AcquireOrCreateDistributedContextAsync(defaultContext);

            // If the required distributed features are not enabled, nothing to do.
            var distributedCache = context?.DistributedCache;
            if (distributedCache == null)
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
            if (!_shellHost.TryGetShellContext(ShellHelper.DefaultShellName, out var defaultContext) ||
                defaultContext.Settings.State != TenantState.Running)
            {
                return;
            }

            // Acquire the distributed context or create a new one if not yet built.
            using var context = await AcquireOrCreateDistributedContextAsync(defaultContext);

            // If the required distributed features are not enabled, nothing to do.
            var distributedCache = context?.DistributedCache;
            if (distributedCache == null)
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
                if (name != ShellHelper.DefaultShellName && !_shellHost.TryGetSettings(name, out _))
                {
                    // Also update the global identifier specifying that a tenant has been created.
                    await distributedCache.SetStringAsync(ShellCreatedIdKey, identifier.ReloadId);
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

        private string ReleaseIdKey(string name) => name + ReleaseIdKeySuffix;
        private string ReloadIdKey(string name) => name + ReloadIdKeySuffix;

        /// <summary>
        /// Creates a distributed context based on the default tenant settings and descriptor.
        /// </summary>
        private async Task<DistributedContext> CreateDistributedContextAsync(ShellContext defaultShell)
        {
            // Capture the descriptor as the blueprint may be set to null right after.
            var descriptor = defaultShell.Blueprint?.Descriptor;
            if (descriptor != null)
            {
                // Using the current shell descritor prevents a database access, and a race condition
                // when resolving `IStore` while the default tenant is activating and does migrations.
                try
                {
                    return new DistributedContext(await _shellContextFactory.CreateDescribedContextAsync(defaultShell.Settings, descriptor));
                }
                catch
                {
                    return null;
                }
            }

            return await CreateDistributedContextAsync(defaultShell.Settings);
        }

        /// <summary>
        /// Creates a distributed context based on the default tenant settings.
        /// </summary>
        private async Task<DistributedContext> CreateDistributedContextAsync(ShellSettings defaultSettings)
        {
            try
            {
                return new DistributedContext(await _shellContextFactory.CreateShellContextAsync(defaultSettings));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the distributed context or creates a new one if the default tenant has changed.
        /// </summary>
        private async Task<DistributedContext> GetOrCreateDistributedContextAsync(ShellContext defaultContext)
        {
            // Check if the default tenant has changed.
            if (_defaultContext != defaultContext)
            {
                var previousContext = _context;

                // Create a new distributed context based on the default tenant.
                _context = await CreateDistributedContextAsync(defaultContext);

                if (_context != null)
                {
                    _defaultContext = defaultContext;
                }

                // Release the previous one.
                previousContext?.Release();
            }

            return _context;
        }

        /// <summary>
        /// Acquires the distributed context or creates a new one if not yet initialized.
        /// </summary>
        private Task<DistributedContext> AcquireOrCreateDistributedContextAsync(ShellContext defaultContext)
        {
            var distributedContext = _context?.Acquire();
            if (distributedContext == null)
            {
                return CreateDistributedContextAsync(defaultContext);
            }

            return Task.FromResult(distributedContext);
        }

        /// <summary>
        /// Gets the next idle time before retrying to read the distributed cache.
        /// </summary>
        private TimeSpan NextIdleTimeBeforeRetry(TimeSpan idleTime, Exception ex)
        {
            if (idleTime < MaxRetryTime)
            {
                // Log an error on each retry, but only before reaching the 'MaxRetryTime', to not fill out the log.
                _logger.LogError(ex, "Unable to read the distributed cache before checking if a tenant has changed.");

                idleTime *= 2;
                if (idleTime > MaxRetryTime)
                {
                    idleTime = MaxRetryTime;
                }
            }

            return idleTime;
        }

        /// <summary>
        /// Tries to wait for the min idle time after the max busy time, returns false if it was cancelled.
        /// </summary>
        private async Task<bool> TryWaitAfterBusyTime(CancellationToken stoppingToken)
        {
            if (DateTime.UtcNow - _busyStartTime > MaxBusyTime)
            {
                if (!await TryWaitAsync(MinIdleTime, stoppingToken))
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
        private async Task<bool> TryWaitAsync(TimeSpan delay, CancellationToken stoppingToken)
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
}
