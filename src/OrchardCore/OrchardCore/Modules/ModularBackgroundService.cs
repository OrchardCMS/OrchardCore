using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Settings;

namespace OrchardCore.Modules;

internal sealed class ModularBackgroundService : BackgroundService
{
    private readonly ConcurrentDictionary<string, BackgroundTaskScheduler> _schedulers = new();
    private readonly ConcurrentDictionary<string, IChangeToken> _changeTokens = new();

    private readonly IShellHost _shellHost;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly BackgroundServiceOptions _options;
    private readonly ILogger _logger;
    private readonly IClock _clock;

    public ModularBackgroundService(
        IShellHost shellHost,
        IHttpContextAccessor httpContextAccessor,
        IOptions<BackgroundServiceOptions> options,
        ILogger<ModularBackgroundService> logger,
        IClock clock)
    {
        _shellHost = shellHost;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
        _logger = logger;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() =>
        {
            _logger.LogInformation("'{ServiceName}' is stopping.", nameof(ModularBackgroundService));
        });

        if (_options.ShellWarmup)
        {
            try
            {
                // Ensure all tenants are pre-loaded.
                await _shellHost.InitializeAsync();
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Failed to warm up the tenants from '{ServiceName}'.", nameof(ModularBackgroundService));
            }
        }

        while (GetRunningShells().Length == 0)
        {
            try
            {
                await Task.Delay(_options.MinimumIdleTime, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        var previousShells = Array.Empty<(string Tenant, long UtcTicks)>();
        while (!stoppingToken.IsCancellationRequested)
        {
            // Init the delay first to be also waited on exception.
            var pollingDelay = Task.Delay(_options.PollingTime, stoppingToken);
            try
            {
                var runningShells = GetRunningShells();
                await UpdateAsync(previousShells, runningShells, stoppingToken);
                await RunAsync(runningShells, stoppingToken);
                previousShells = runningShells;
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Error while executing '{ServiceName}'.", nameof(ModularBackgroundService));
            }

            await WaitAsync(pollingDelay, stoppingToken);
        }
    }

    private async Task RunAsync(IEnumerable<(string Tenant, long UtcTicks)> runningShells, CancellationToken stoppingToken)
    {
        await Parallel.ForEachAsync(GetShellsToRun(runningShells), async (tenant, cancellationToken) =>
        {
            // Check if the shell is still registered and running.
            if (!_shellHost.TryGetShellContext(tenant, out var shell) || !shell.Settings.IsRunning())
            {
                return;
            }

            // Create a new 'HttpContext' to be used in the background.
            _httpContextAccessor.HttpContext = shell.CreateHttpContext();

            var schedulers = GetSchedulersToRun(tenant);
            foreach (var scheduler in schedulers)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                // Try to create a shell scope on this shell context.
                var (shellScope, success) = await _shellHost.TryGetScopeAsync(shell.Settings.Name);
                if (!success)
                {
                    break;
                }

                // Check if the shell has no pipeline and should not be warmed up.
                if (!_options.ShellWarmup && !shellScope.ShellContext.HasPipeline())
                {
                    await shellScope.TerminateShellAsync();
                    break;
                }

                var locked = false;
                ILocker locker = null;
                try
                {
                    // Try to acquire a lock before using the scope, so that a next process gets the last committed data.
                    var distributedLock = shellScope.ShellContext.ServiceProvider.GetRequiredService<IDistributedLock>();
                    (locker, locked) = await distributedLock.TryAcquireBackgroundTaskLockAsync(scheduler.Settings);
                    if (!locked)
                    {
                        await shellScope.TerminateShellAsync();
                        _logger.LogInformation("Timeout to acquire a lock on background task '{TaskName}' on tenant '{TenantName}'.", scheduler.Name, tenant);
                        break;
                    }
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    await shellScope.TerminateShellAsync();
                    _logger.LogError(ex, "Failed to acquire a lock on background task '{TaskName}' on tenant '{TenantName}'.", scheduler.Name, tenant);
                    break;
                }

                await using var acquiredLock = locker;

                await shellScope.UsingAsync(async scope =>
                {
                    var taskName = scheduler.Name;

                    var task = scope.ServiceProvider.GetServices<IBackgroundTask>().GetTaskByName(taskName);
                    if (task is null)
                    {
                        return;
                    }

                    var siteService = scope.ServiceProvider.GetService<ISiteService>();
                    if (siteService is not null)
                    {
                        try
                        {
                            // Use the base url, if defined, to override the 'Scheme', 'Host' and 'PathBase'.
                            _httpContextAccessor.HttpContext.SetBaseUrl((await siteService.GetSiteSettingsAsync()).BaseUrl);
                        }
                        catch (Exception ex) when (!ex.IsFatal())
                        {
                            _logger.LogError(ex, "Error while getting the base url from the site settings of the tenant '{TenantName}'.", tenant);
                        }
                    }

                    try
                    {
                        if (scheduler.Settings.UsePipeline)
                        {
                            if (!scope.ShellContext.HasPipeline())
                            {
                                // Build the shell pipeline to configure endpoint data sources.
                                await scope.ShellContext.BuildPipelineAsync();
                            }

                            // Run the pipeline to make the 'HttpContext' aware of endpoints.
                            await scope.ShellContext.Pipeline.Invoke(_httpContextAccessor.HttpContext);
                        }
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        _logger.LogError(ex, "Error while running in the background the pipeline of tenant '{TenantName}'.", tenant);
                    }

                    var context = new BackgroundTaskEventContext(taskName, scope);
                    var handlers = scope.ServiceProvider.GetServices<IBackgroundTaskEventHandler>();

                    await handlers.InvokeAsync((handler, context, token) => handler.ExecutingAsync(context, token), context, stoppingToken, _logger);

                    try
                    {
                        _logger.LogInformation("Start processing background task '{TaskName}' on tenant '{TenantName}'.", taskName, tenant);

                        scheduler.Run();
                        await task.DoWorkAsync(scope.ServiceProvider, stoppingToken);

                        _logger.LogInformation("Finished processing background task '{TaskName}' on tenant '{TenantName}'.", taskName, tenant);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        _logger.LogError(ex, "Error while processing background task '{TaskName}' on tenant '{TenantName}'.", taskName, tenant);
                        context.Exception = ex;

                        await scope.HandleExceptionAsync(ex);
                    }

                    await handlers.InvokeAsync((handler, context, token) => handler.ExecutedAsync(context, token), context, stoppingToken, _logger);
                });
            }

            // Clear the 'HttpContext' for this async flow.
            _httpContextAccessor.HttpContext = null;
        });
    }

    private async Task UpdateAsync(
        (string Tenant, long UtcTicks)[] previousShells,
        (string Tenant, long UtcTicks)[] runningShells, CancellationToken stoppingToken)
    {
        var referenceTime = DateTime.UtcNow;

        await Parallel.ForEachAsync(GetShellsToUpdate(previousShells, runningShells), async (tenant, cancellationToken) =>
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            // Check if the shell is still registered and running.
            if (!_shellHost.TryGetShellContext(tenant, out var shell) || !shell.Settings.IsRunning())
            {
                return;
            }

            // Try to create a shell scope on this shell context.
            var (shellScope, success) = await _shellHost.TryGetScopeAsync(shell.Settings.Name);
            if (!success)
            {
                return;
            }

            // Check if the shell has no pipeline and should not be warmed up.
            if (!_options.ShellWarmup && !shellScope.ShellContext.HasPipeline())
            {
                await shellScope.TerminateShellAsync();
                return;
            }

            // Create a new 'HttpContext' to be used in the background.
            _httpContextAccessor.HttpContext = shell.CreateHttpContext();

            await shellScope.UsingAsync(async scope =>
            {
                var tasks = scope.ServiceProvider.GetServices<IBackgroundTask>();

                CleanSchedulers(tenant, tasks);

                if (!tasks.Any())
                {
                    return;
                }

                var settingsProvider = scope.ServiceProvider.GetService<IBackgroundTaskSettingsProvider>();
                _changeTokens[tenant] = settingsProvider?.ChangeToken ?? NullChangeToken.Singleton;

                ITimeZone timeZone = null;
                var siteService = scope.ServiceProvider.GetService<ISiteService>();
                if (siteService is not null)
                {
                    try
                    {
                        timeZone = _clock.GetTimeZone((await siteService.GetSiteSettingsAsync()).TimeZoneId);
                    }
                    catch (Exception ex) when (!ex.IsFatal())
                    {
                        _logger.LogError(ex, "Error while getting the time zone from the site settings of the tenant '{TenantName}'.", tenant);
                    }
                }

                foreach (var task in tasks)
                {
                    var taskName = task.GetTaskName();
                    var tenantTaskName = tenant + taskName;
                    if (!_schedulers.TryGetValue(tenantTaskName, out var scheduler))
                    {
                        _schedulers[tenantTaskName] = scheduler = new BackgroundTaskScheduler(tenant, taskName, referenceTime, _clock);
                    }

                    scheduler.TimeZone = timeZone;
                    if (!scheduler.Released && scheduler.Updated)
                    {
                        continue;
                    }

                    BackgroundTaskSettings settings = null;
                    if (settingsProvider is not null)
                    {
                        try
                        {
                            settings = await settingsProvider.GetSettingsAsync(task);
                        }
                        catch (Exception ex) when (!ex.IsFatal())
                        {
                            _logger.LogError(ex, "Error while updating settings of background task '{TaskName}' on tenant '{TenantName}'.", taskName, tenant);
                        }
                    }

                    settings ??= task.GetDefaultSettings();
                    if (scheduler.Released || !scheduler.Settings.Schedule.Equals(settings.Schedule, StringComparison.Ordinal))
                    {
                        scheduler.ReferenceTime = referenceTime;
                    }

                    scheduler.Settings = settings;
                    scheduler.Released = false;
                    scheduler.Updated = true;
                }
            });

            // Clear the 'HttpContext' for this async flow.
            _httpContextAccessor.HttpContext = null;
        });
    }

    private async Task WaitAsync(Task pollingDelay, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(_options.MinimumIdleTime, stoppingToken);
            await pollingDelay;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private (string Tenant, long UtcTicks)[] GetRunningShells() => _shellHost
        .ListShellContexts()
        .Where(shell => shell.Settings.IsRunning() && (_options.ShellWarmup || shell.HasPipeline()))
        .Select(shell => (shell.Settings.Name, shell.UtcTicks))
        .ToArray();

    private string[] GetShellsToRun(IEnumerable<(string Tenant, long UtcTicks)> shells)
    {
        var tenantsToRun = _schedulers
            .Where(scheduler => scheduler.Value.CanRun())
            .Select(scheduler => scheduler.Value.Tenant)
            .Distinct()
            .ToArray();

        return shells
            .Select(shell => shell.Tenant)
            .Where(tenant => tenantsToRun.Contains(tenant))
            .ToArray();
    }

    private string[] GetShellsToUpdate((string Tenant, long UtcTicks)[] previousShells, (string Tenant, long UtcTicks)[] runningShells)
    {
        var previousTenants = previousShells.Select(shell => shell.Tenant);

        var releasedTenants = new List<string>();
        foreach (var (tenant, utcTicks) in previousShells)
        {
            if (_shellHost.TryGetShellContext(tenant, out var existing) &&
                existing.UtcTicks == utcTicks &&
                !existing.Released)
            {
                continue;
            }

            releasedTenants.Add(tenant);
        }

        if (releasedTenants.Count > 0)
        {
            UpdateSchedulers(releasedTenants.ToArray(), scheduler => scheduler.Released = true);
        }

        var changedTenants = _changeTokens.Where(token => token.Value.HasChanged).Select(token => token.Key).ToArray();
        if (changedTenants.Length > 0)
        {
            UpdateSchedulers(changedTenants, scheduler => scheduler.Updated = false);
        }

        var runningTenants = runningShells.Select(shell => shell.Tenant);
        var validTenants = previousTenants.Except(releasedTenants).Except(changedTenants);
        var tenantsToUpdate = runningTenants.Except(validTenants).ToArray();

        return runningTenants.Where(tenant => tenantsToUpdate.Contains(tenant)).ToArray();
    }

    private BackgroundTaskScheduler[] GetSchedulersToRun(string tenant) => _schedulers
        .Where(scheduler => scheduler.Value.Tenant == tenant && scheduler.Value.CanRun())
        .Select(scheduler => scheduler.Value)
        .ToArray();

    private void UpdateSchedulers(string[] tenants, Action<BackgroundTaskScheduler> action)
    {
        var keys = _schedulers.Where(kv => tenants.Contains(kv.Value.Tenant)).Select(kv => kv.Key).ToArray();
        foreach (var key in keys)
        {
            if (_schedulers.TryGetValue(key, out var scheduler))
            {
                action(scheduler);
            }
        }
    }

    private void CleanSchedulers(string tenant, IEnumerable<IBackgroundTask> tasks)
    {
        var validKeys = tasks.Select(task => tenant + task.GetTaskName()).ToArray();

        var keys = _schedulers.Where(kv => kv.Value.Tenant == tenant).Select(kv => kv.Key).ToArray();
        foreach (var key in keys)
        {
            if (!validKeys.Contains(key))
            {
                _schedulers.TryRemove(key, out _);
            }
        }
    }
}
