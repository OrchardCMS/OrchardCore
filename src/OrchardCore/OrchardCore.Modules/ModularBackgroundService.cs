using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;

namespace OrchardCore.Modules
{
    internal class ModularBackgroundService : Internal.BackgroundService, IModularBackgroundService
    {
        private static TimeSpan PollingTime = TimeSpan.FromMinutes(1);
        private static TimeSpan MinIdleTime = TimeSpan.FromSeconds(10);
        private static readonly object _synLock = new object();

        private CancellationTokenSource _updateSource = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, BackgroundTaskScheduler> _schedulers =
            new ConcurrentDictionary<string, BackgroundTaskScheduler>();

        private readonly IShellHost _shellHost;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModularBackgroundService(
            IShellHost shellHost,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ModularBackgroundService> logger)
        {
            _shellHost = shellHost;
            _httpContextAccessor = httpContextAccessor;
            Logger = logger;
        }

        public bool IsRunning { get; private set; }
        public ILogger Logger { get; set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                Logger.LogDebug($"{nameof(ModularBackgroundService)} is stopping.");
            });

            IEnumerable<ShellContext> shells = null;
            while ((shells?.Count() ?? 0) < 1)
            {
                await Task.Delay(MinIdleTime, stoppingToken);
                shells = GetRunningShells();
            }

            IsRunning = true;

            var updateSource = CancellationTokenSource.CreateLinkedTokenSource(
                _updateSource.Token, stoppingToken);

            IEnumerable<ShellContext> previousShells = Enumerable.Empty<ShellContext>();
            bool updateSchedulers = true;

            while (!stoppingToken.IsCancellationRequested)
            {
                var pollingDelay = Task.Delay(PollingTime, updateSource.Token);

                shells = GetRunningShells();

                if (updateSchedulers)
                {
                    await UpdateSchedulers(previousShells, shells, stoppingToken);
                    updateSchedulers = false;
                }

                previousShells = shells;

                await GetShellsToRun(shells).ForEachAsync(async shell =>
                {
                    var tenant = shell.Settings?.Name;
                    var schedulers = GetTenantSchedulersToRun(tenant);

                    _httpContextAccessor.HttpContext = shell.GetBackgroundHttpContext();

                    foreach (var scheduler in schedulers)
                    {
                        var taskName = scheduler.Name;

                        if (shell.Released || stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        using (var scope = shell.EnterServiceScope())
                        {
                            var task = scope.GetBackgroundTaskByTypeName(taskName);

                            if (task == null)
                            {
                                continue;
                            }

                            try
                            {
                                if (Logger.IsEnabled(LogLevel.Information))
                                {
                                    Logger.LogInformation(
                                        "Start processing background task \"{0}\" on tenant \"{1}\".",
                                        tenant, taskName);
                                }

                                scheduler.Run();

                                await task.DoWorkAsync(scope.ServiceProvider, stoppingToken);

                                scheduler.Idle();

                                if (Logger.IsEnabled(LogLevel.Information))
                                {
                                    Logger.LogInformation(
                                        "Finished processing background task \"{0}\" on tenant \"{1}\".",
                                        tenant, taskName);
                                }
                            }

                            catch (Exception ex)
                            {
                                scheduler.Fault(ex);

                                if (Logger.IsEnabled(LogLevel.Error))
                                {
                                    Logger.LogError(ex,
                                        "Error while processing background task \"{0}\" on tenant \"{1}\".",
                                        tenant, taskName);
                                }
                            }
                        }
                    }
                });

                var minIdleDelay = Task.Delay(MinIdleTime, updateSource.Token);

                try
                {
                    while (!minIdleDelay.IsCompleted || !pollingDelay.IsCompleted)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), updateSource.Token);

                        if (shells.Any(s => s.Released) || shells.Count() != GetRunningShells().Count())
                        {
                            _updateSource.Cancel();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }

                if (_updateSource.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
                {
                    lock (_synLock)
                    {
                        _updateSource = new CancellationTokenSource();
                    }

                    updateSource.Dispose();
                    updateSource = CancellationTokenSource.CreateLinkedTokenSource(
                        _updateSource.Token, stoppingToken);

                    updateSchedulers = true;
                }
            }

            updateSource.Dispose();
            IsRunning = false;
        }

        protected async Task UpdateSchedulers(IEnumerable<ShellContext> previousShells,
            IEnumerable<ShellContext> runningShells, CancellationToken stoppingToken)
        {
            var referenceTime = DateTime.UtcNow;

            var tenantsToClean = previousShells.Where(s => s.Released).Select(s => s.Settings?.Name);

            if (tenantsToClean.Any())
            {
                CleanSchedulers(tenantsToClean);
            }

            var runningTenants = runningShells.Select(s => s.Settings?.Name);
            var previousTenants = previousShells.Where(s => !s.Released).Select(s => s.Settings?.Name);

            var tenantsToAdd = runningTenants.Where(t => !previousTenants.Contains(t));
            var tenantsToUpdate = _schedulers.Where(s => !s.Value.Updated).Select(s => s.Value.Tenant);
            var tenantsToAddOrUpdate = tenantsToAdd.Concat(tenantsToUpdate).Distinct();

            var shellsToUpdate = runningShells.Where(s => tenantsToAddOrUpdate.Contains(s.Settings?.Name));

            await shellsToUpdate.ForEachAsync(async shell =>
            {
                IEnumerable<Type> taskTypes;
                var tenant = shell.Settings?.Name;

                _httpContextAccessor.HttpContext = shell.GetBackgroundHttpContext();

                if (shell.Released || stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                using (var scope = shell.EnterServiceScope())
                {
                    taskTypes = scope.GetBackgroundTaskTypes();
                    CleanTenantSchedulers(tenant, taskTypes);

                    if (!taskTypes.Any())
                    {
                        return;
                    }

                    foreach (var taskType in taskTypes)
                    {
                        if (shell.Released || stoppingToken.IsCancellationRequested)
                        {
                            return;
                        }

                        var taskName = taskType.FullName;

                        if (!_schedulers.TryGetValue(tenant + taskName, out BackgroundTaskScheduler scheduler))
                        {
                            _schedulers[tenant + taskName] = scheduler = new BackgroundTaskScheduler(tenant, taskName, referenceTime);
                        }

                        if (scheduler.Updated)
                        {
                            continue;
                        }

                        try
                        {
                            var settings = await scope.GetBackgroundTaskSettingsAsync(taskType);

                            if (!scheduler.Settings.Schedule.Equals(settings.Schedule))
                            {
                                scheduler.ReferenceTime = referenceTime;
                            }

                            scheduler.Settings = settings.Clone();
                            scheduler.Updated = true;
                        }

                        catch (Exception ex)
                        {
                            scheduler.Fault(ex);

                            if (Logger.IsEnabled(LogLevel.Error))
                            {
                                Logger.LogError(ex,
                                    "Error while updating settings of background task \"{0}\" on tenant \"{1}\".",
                                    tenant, taskName);
                            }
                        }
                    }
                }
            });
        }

        public Task UpdateAsync(string tenant, string taskName)
        {
            if (_schedulers.TryGetValue(tenant + taskName, out BackgroundTaskScheduler scheduler))
            {
                scheduler = scheduler.Clone();
                scheduler.Updated = false;
                _schedulers[tenant + taskName] = scheduler;
            }

            lock (_synLock)
            {
                _updateSource.Cancel();
            }

            return Task.CompletedTask;
        }

        public void Command(string tenant, string taskName, BackgroundTaskScheduler.CommandCode code)
        {
            if (_schedulers.TryRemove(tenant + taskName, out BackgroundTaskScheduler scheduler))
            {
                _schedulers[tenant + taskName] = scheduler.Command(code);
            }
        }

        public Task<BackgroundTaskSettings> GetSettingsAsync(string tenant, string taskName)
        {
            if (_schedulers.TryGetValue(tenant + taskName, out BackgroundTaskScheduler scheduler))
            {
                return Task.FromResult(scheduler.Settings.Clone());
            }

            return Task.FromResult(BackgroundTaskSettings.None);
        }

        public Task<IEnumerable<BackgroundTaskSettings>> GetSettingsAsync(string tenant)
        {
            return Task.FromResult(_schedulers.Where(kv => kv.Value.Tenant == tenant)
                .Select(kv => kv.Value.Settings.Clone()));
        }

        public Task<BackgroundTaskState> GetStateAsync(string tenant, string taskName)
        {
            if (_schedulers.TryGetValue(tenant + taskName, out BackgroundTaskScheduler scheduler))
            {
                return Task.FromResult(scheduler.State.Clone());
            }

            return Task.FromResult(BackgroundTaskState.Undefined);
        }

        public Task<IEnumerable<BackgroundTaskState>> GetStatesAsync(string tenant)
        {
            return Task.FromResult(_schedulers.Where(kv => kv.Value.Tenant == tenant)
                .Select(kv => kv.Value.State.Clone()));
        }

        private IEnumerable<ShellContext> GetRunningShells()
        {
            return _shellHost.ListShellContexts()?.Where(s => s.Settings?.State == TenantState.Running)
                .ToArray() ?? Enumerable.Empty<ShellContext>();
        }

        private IEnumerable<ShellContext> GetShellsToRun(IEnumerable<ShellContext> shells)
        {
            var schedulers = _schedulers.Where(s => s.Value.CanRun());
            var tenants = schedulers.Select(s => s.Value.Tenant).ToArray();
            return shells.Where(s => tenants.Contains(s.Settings?.Name));
        }

        private void CleanSchedulers(IEnumerable<string> tenants)
        {
            var keys = _schedulers.Where(kv => tenants.Contains(kv.Value.Tenant)).Select(kv => kv.Key).ToArray();

            foreach (var key in keys)
            {
                _schedulers.TryRemove(key, out var scheduler);
            }
        }

        private void CleanTenantSchedulers(string tenant, IEnumerable<Type> taskTypes)
        {
            var validKeys = taskTypes.Select(type => tenant + type.FullName);

            var keys = _schedulers.Where(kv => kv.Value.Tenant == tenant).Select(kv => kv.Key).ToArray();

            foreach (var key in keys)
            {
                if (!validKeys.Contains(key))
                {
                    _schedulers.TryRemove(key, out var scheduler);
                }
            }
        }

        private IEnumerable<BackgroundTaskScheduler> GetTenantSchedulersToRun(string tenant)
        {
            return _schedulers.Where(s => s.Value.Tenant == tenant && s.Value.CanRun()).Select(s => s.Value);
        }
    }

    internal static class EnumerableExtensions
    {
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
        {
            var partitionCount = System.Environment.ProcessorCount;

            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(partitionCount)
                select Task.Run(async delegate
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await body(partition.Current);
                        }
                    }
                }));
        }
    }

    internal static class ShellExtensions
    {
        public static HttpContext GetBackgroundHttpContext(this ShellContext shell)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString(shell.Settings?.RequestUrlHost ?? "localhost");
            httpContext.Request.Path = "/" + shell.Settings?.RequestUrlPrefix ?? "";
            httpContext.Items["IsBackground"] = true;
            return httpContext;
        }
    }

    internal static class ServiceScopeExtensions
    {
        public static IEnumerable<Type> GetBackgroundTaskTypes(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().Select(t => t.GetType());
        }

        public static IBackgroundTask GetBackgroundTaskByTypeName(this IServiceScope scope, string type)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().FirstOrDefault(t => t.GetType().FullName == type);
        }

        public static async Task<BackgroundTaskSettings> GetBackgroundTaskSettingsAsync(this IServiceScope scope, Type type)
        {
            var providers = scope.ServiceProvider.GetService<IOptions<BackgroundTaskOptions>>()
                .Value.SettingsProviders;

            foreach (var provider in providers.OrderBy(p => p.Order))
            {
                var settings = await provider.GetSettingsAsync(type);

                if (settings != null && settings != BackgroundTaskSettings.None)
                {
                    return settings;
                }
            }

            return new BackgroundTaskSettings() { Name = type.FullName };
        }
    }
}