using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Locking.Distributed;
using OrchardCore.Settings;

namespace OrchardCore.Modules
{
    internal class ModularBackgroundService : BackgroundService
    {
        private static readonly TimeSpan PollingTime = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan MinIdleTime = TimeSpan.FromSeconds(10);

        private readonly ConcurrentDictionary<string, BackgroundTaskScheduler> _schedulers =
            new ConcurrentDictionary<string, BackgroundTaskScheduler>();

        private readonly ConcurrentDictionary<string, IChangeToken> _changeTokens =
            new ConcurrentDictionary<string, IChangeToken>();

        private readonly IShellHost _shellHost;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly IClock _clock;

        public ModularBackgroundService(
            IShellHost shellHost,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ModularBackgroundService> logger,
            IClock clock)
        {
            _shellHost = shellHost;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _clock = clock;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("'{ServiceName}' is stopping.", nameof(ModularBackgroundService));
            });

            while (GetRunningShells().Count() < 1)
            {
                try
                {
                    await Task.Delay(MinIdleTime, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            var previousShells = Enumerable.Empty<ShellContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var runningShells = GetRunningShells();
                    await UpdateAsync(previousShells, runningShells, stoppingToken);
                    previousShells = runningShells;

                    var pollingDelay = Task.Delay(PollingTime, stoppingToken);

                    await RunAsync(runningShells, stoppingToken);
                    await WaitAsync(pollingDelay, stoppingToken);
                }
                catch (Exception ex) when (!ex.IsFatal())
                {
                    _logger.LogError(ex, "Error while executing '{ServiceName}'", nameof(ModularBackgroundService));
                }
            }
        }

        private async Task RunAsync(IEnumerable<ShellContext> runningShells, CancellationToken stoppingToken)
        {
            await GetShellsToRun(runningShells).ForEachAsync(async shell =>
            {
                var tenant = shell.Settings.Name;

                var schedulers = GetSchedulersToRun(tenant);

                _httpContextAccessor.HttpContext = shell.CreateHttpContext();

                foreach (var scheduler in schedulers)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var shellScope = await _shellHost.GetScopeAsync(shell.Settings);

                    if (shellScope.ShellContext.Pipeline == null)
                    {
                        break;
                    }

                    var distributedLock = shellScope.ShellContext.ServiceProvider.GetRequiredService<IDistributedLock>();

                    // Try to acquire a lock before using the scope, so that a next process gets the last committed data.
                    (var locker, var locked) = await distributedLock.TryAcquireBackgroundTaskLockAsync(scheduler.Settings);
                    if (!locked)
                    {
                        _logger.LogInformation("Timeout to acquire a lock on background task '{TaskName}' on tenant '{TenantName}'.", scheduler.Name, tenant);
                        return;
                    }

                    await using var acquiredLock = locker;

                    await shellScope.UsingAsync(async scope =>
                    {
                        var taskName = scheduler.Name;

                        var task = scope.ServiceProvider.GetServices<IBackgroundTask>().GetTaskByName(taskName);

                        if (task == null)
                        {
                            return;
                        }

                        var siteService = scope.ServiceProvider.GetService<ISiteService>();
                        if (siteService != null)
                        {
                            try
                            {
                                _httpContextAccessor.HttpContext.SetBaseUrl((await siteService.GetSiteSettingsAsync()).BaseUrl);
                            }
                            catch (Exception ex) when (!ex.IsFatal())
                            {
                                _logger.LogError(ex, "Error while getting the base url from the site settings of the tenant '{TenantName}'.", tenant);
                            }
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
            });
        }

        private async Task UpdateAsync(IEnumerable<ShellContext> previousShells, IEnumerable<ShellContext> runningShells, CancellationToken stoppingToken)
        {
            var referenceTime = DateTime.UtcNow;

            await GetShellsToUpdate(previousShells, runningShells).ForEachAsync(async shell =>
            {
                var tenant = shell.Settings.Name;

                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                _httpContextAccessor.HttpContext = shell.CreateHttpContext();

                var shellScope = await _shellHost.GetScopeAsync(shell.Settings);

                if (shellScope.ShellContext.Pipeline == null)
                {
                    return;
                }

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
                    if (siteService != null)
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

                        if (!_schedulers.TryGetValue(tenant + taskName, out var scheduler))
                        {
                            _schedulers[tenant + taskName] = scheduler = new BackgroundTaskScheduler(tenant, taskName, referenceTime, _clock);
                        }

                        scheduler.TimeZone = timeZone;

                        if (!scheduler.Released && scheduler.Updated)
                        {
                            continue;
                        }

                        BackgroundTaskSettings settings = null;

                        if (settingsProvider != null)
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

                        if (scheduler.Released || !scheduler.Settings.Schedule.Equals(settings.Schedule))
                        {
                            scheduler.ReferenceTime = referenceTime;
                        }

                        scheduler.Settings = settings;
                        scheduler.Released = false;
                        scheduler.Updated = true;
                    }
                });
            });
        }

        private async Task WaitAsync(Task pollingDelay, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(MinIdleTime, stoppingToken);
                await pollingDelay;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private IEnumerable<ShellContext> GetRunningShells()
        {
            return _shellHost.ListShellContexts().Where(s => s.Settings.State == TenantState.Running && s.Pipeline != null).ToArray();
        }

        private IEnumerable<ShellContext> GetShellsToRun(IEnumerable<ShellContext> shells)
        {
            var tenantsToRun = _schedulers.Where(s => s.Value.CanRun()).Select(s => s.Value.Tenant).Distinct().ToArray();
            return shells.Where(s => tenantsToRun.Contains(s.Settings.Name)).ToArray();
        }

        private IEnumerable<ShellContext> GetShellsToUpdate(IEnumerable<ShellContext> previousShells, IEnumerable<ShellContext> runningShells)
        {
            var released = previousShells.Where(s => s.Released).Select(s => s.Settings.Name).ToArray();

            if (released.Any())
            {
                UpdateSchedulers(released, s => s.Released = true);
            }

            var changed = _changeTokens.Where(t => t.Value.HasChanged).Select(t => t.Key).ToArray();

            if (changed.Any())
            {
                UpdateSchedulers(changed, s => s.Updated = false);
            }

            var valid = previousShells.Select(s => s.Settings.Name).Except(released).Except(changed);
            var tenantsToUpdate = runningShells.Select(s => s.Settings.Name).Except(valid).ToArray();

            return runningShells.Where(s => tenantsToUpdate.Contains(s.Settings.Name)).ToArray();
        }

        private IEnumerable<BackgroundTaskScheduler> GetSchedulersToRun(string tenant)
        {
            return _schedulers.Where(s => s.Value.Tenant == tenant && s.Value.CanRun()).Select(s => s.Value).ToArray();
        }

        private void UpdateSchedulers(IEnumerable<string> tenants, Action<BackgroundTaskScheduler> action)
        {
            var keys = _schedulers.Where(kv => tenants.Contains(kv.Value.Tenant)).Select(kv => kv.Key).ToArray();

            foreach (var key in keys)
            {
                if (_schedulers.TryGetValue(key, out BackgroundTaskScheduler scheduler))
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
                    _schedulers.TryRemove(key, out var scheduler);
                }
            }
        }
    }

    internal static class HttpContextExtensions
    {
        public static void SetBaseUrl(this HttpContext context, string baseUrl)
        {
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            {
                context.Request.Scheme = uri.Scheme;
                context.Request.Host = new HostString(uri.Host, uri.Port);
                context.Request.PathBase = uri.AbsolutePath;

                if (!String.IsNullOrWhiteSpace(uri.Query))
                {
                    context.Request.QueryString = new QueryString(uri.Query);
                }
            }
        }
    }

    internal static class ShellExtensions
    {
        public static HttpContext CreateHttpContext(this ShellContext shell)
        {
            var context = shell.Settings.CreateHttpContext();

            context.Features.Set(new ShellContextFeature
            {
                ShellContext = shell,
                OriginalPathBase = String.Empty,
                OriginalPath = "/"
            });

            return context;
        }

        public static HttpContext CreateHttpContext(this ShellSettings settings)
        {
            var context = new DefaultHttpContext().UseShellScopeServices();

            context.Request.Scheme = "https";

            var urlHost = settings.RequestUrlHost?.Split(new[] { ',', ' ' },
                StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            context.Request.Host = new HostString(urlHost ?? "localhost");

            if (!String.IsNullOrWhiteSpace(settings.RequestUrlPrefix))
            {
                context.Request.PathBase = "/" + settings.RequestUrlPrefix;
            }

            context.Request.Path = "/";
            context.Items["IsBackground"] = true;

            return context;
        }
    }
}
