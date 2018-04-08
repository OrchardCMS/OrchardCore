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

            while (GetRunningShells().Count() < 1)
            {
                await Task.Delay(MinIdleTime, stoppingToken);
            }

            IsRunning = true;

            var previousTenants = Enumerable.Empty<Tenant>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var runningTenants = GetRunningTenants();
                await UpdateAsync(previousTenants, runningTenants, stoppingToken);
                previousTenants = runningTenants;

                var pollingDelay = Task.Delay(PollingTime, stoppingToken);

                await RunAsync(runningTenants, stoppingToken);
                await WaitAsync(pollingDelay, stoppingToken);
            }

            IsRunning = false;
        }

        private async Task RunAsync(IEnumerable<Tenant> runningTenants, CancellationToken stoppingToken)
        {
            await GetTenantsToRun(runningTenants).ForEachAsync(async tenant =>
            {
                var shell = tenant.Shell;

                var schedulers = GetSchedulersToRun(tenant.Name);

                _httpContextAccessor.HttpContext = shell.GetHttpContext();

                foreach (var scheduler in schedulers)
                {
                    var taskName = scheduler.Name;

                    if (shell.Released || stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    using (var scope = shell.EnterServiceScope())
                    {
                        var task = scope.GetTaskByTypeName(taskName);

                        if (task == null)
                        {
                            continue;
                        }

                        try
                        {
                            Logger.Information($"Start processing background task {taskName} on tenant {tenant.Name}.");

                            scheduler.Run();

                            await task.DoWorkAsync(scope.ServiceProvider, stoppingToken);

                            scheduler.Idle();

                            Logger.Information($"Finished processing background task {taskName} on tenant {tenant.Name}.");
                        }

                        catch (Exception e)
                        {
                            scheduler.Fault(e);

                            Logger.Error(e, $"Error while processing background task {taskName} on tenant {tenant.Name}.");
                        }
                    }
                }
            });
        }

        private async Task UpdateAsync(IEnumerable<Tenant> previousTenants, IEnumerable<Tenant> runningTenants, CancellationToken stoppingToken)
        {
            var referenceTime = DateTime.UtcNow;

            await GetTenantsToUpdate(previousTenants, runningTenants).ForEachAsync(async tenant =>
            {
                var shell = tenant.Shell;

                IEnumerable<Type> taskTypes;

                if (shell.Released || stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                using (var scope = shell.EnterServiceScope())
                {
                    taskTypes = scope.GetTaskTypes();
                    CleanSchedulers(tenant.Name, taskTypes);

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

                        if (!_schedulers.TryGetValue(tenant.Name + taskName, out BackgroundTaskScheduler scheduler))
                        {
                            _schedulers[tenant.Name + taskName] = scheduler = new BackgroundTaskScheduler(tenant.Name, taskName, referenceTime);
                        }

                        if (scheduler.Updated && !scheduler.Released)
                        {
                            continue;
                        }

                        try
                        {
                            scheduler = scheduler.Clone();

                            var settings = await scope.GetTaskSettingsAsync(taskType);

                            if (scheduler.Released || !scheduler.Settings.Schedule.Equals(settings.Schedule))
                            {
                                scheduler.ReferenceTime = referenceTime;
                            }

                            scheduler.Settings = settings.Clone();
                            scheduler.Released = false;
                            scheduler.Updated = true;

                            _schedulers[tenant.Name + taskName] = scheduler;
                        }

                        catch (Exception e)
                        {
                            scheduler.Fault(e);
                            Logger.Error(e, $"Error while updating settings of background task {taskName} on tenant {tenant.Name}.");
                        }
                    }
                }
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

        public async Task UpdateAsync(string tenant)
        {
            var shell = GetRunningShells().FirstOrDefault(s => s.Settings?.Name == tenant);

            if (shell != null)
            {
                await UpdateAsync(Enumerable.Empty<Tenant>(), new Tenant[] { new Tenant(shell) }, CancellationToken.None);
                var schedulers = GetSchedulersToRun(tenant);
            }
        }

        public Task UpdateAsync(string tenant, string taskName)
        {
            if (_schedulers.TryGetValue(tenant + taskName, out BackgroundTaskScheduler scheduler))
            {
                scheduler = scheduler.Clone();
                scheduler.Updated = false;
                _schedulers[tenant + taskName] = scheduler;
            }

            return UpdateAsync(tenant);
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
                .Select(kv => kv.Value.Settings.Clone()).ToArray().AsEnumerable());
        }

        public Task<BackgroundTaskState> GetStateAsync(string tenant, string taskName)
        {
            if (_schedulers.TryGetValue(tenant + taskName, out BackgroundTaskScheduler scheduler) && !scheduler.Released)
            {
                return Task.FromResult(scheduler.State.Clone());
            }

            return Task.FromResult(BackgroundTaskState.Undefined);
        }

        public Task<IEnumerable<BackgroundTaskState>> GetStatesAsync(string tenant)
        {
            return Task.FromResult(_schedulers.Where(kv => kv.Value.Tenant == tenant && !kv.Value.Released)
                .Select(kv => kv.Value.State.Clone()).ToArray().AsEnumerable());
        }

        private class Tenant
        {
            public Tenant(ShellContext shell)
            {
                Name = shell.Settings?.Name;
                Shell = shell;
            }

            public string Name { get; }
            public ShellContext Shell { get; }
        }

        private IEnumerable<ShellContext> GetRunningShells()
        {
            return _shellHost.ListShellContexts()?
                .Where(s => s.Settings?.State == TenantState.Running && !s.Released && (s.IsActivated || s.ActiveScopes == 0))
                .ToArray() ?? Enumerable.Empty<ShellContext>();
        }

        private IEnumerable<Tenant> GetRunningTenants()
        {
            return GetRunningShells().Select(s => new Tenant(s)).ToArray();
        }

        private IEnumerable<Tenant> GetTenantsToRun(IEnumerable<Tenant> tenants)
        {
            var tenantsToRun = _schedulers.Where(s => s.Value.CanRun()).Select(s => s.Value.Tenant).Distinct().ToArray();
            return tenants.Where(t => tenantsToRun.Contains(t.Name)).ToArray();
        }

        private IEnumerable<Tenant> GetTenantsToUpdate(IEnumerable<Tenant> previousTenants, IEnumerable<Tenant> runningTenants)
        {
            var tenantsToRelease = previousTenants.Where(t => t.Shell.Released).Select(t => t.Name);

            if (tenantsToRelease.Any())
            {
                ReleaseSchedulers(tenantsToRelease);
            }

            var validTenants = previousTenants.Select(t => t.Name).Except(tenantsToRelease);
            var tenantsToadd = runningTenants.Select(t => t.Name).Except(validTenants).ToArray();

            return runningTenants.Where(t => tenantsToadd.Contains(t.Name)).ToArray();
        }

        private IEnumerable<BackgroundTaskScheduler> GetSchedulersToRun(string tenant)
        {
            return _schedulers.Where(s => s.Value.Tenant == tenant && s.Value.CanRun()).Select(s => s.Value).ToArray();
        }

        private void ReleaseSchedulers(IEnumerable<string> tenants)
        {
            var keys = _schedulers.Where(kv => tenants.Contains(kv.Value.Tenant)).Select(kv => kv.Key).ToArray();

            foreach (var key in keys)
            {
                if (_schedulers.TryGetValue(key, out BackgroundTaskScheduler scheduler))
                {
                    scheduler = scheduler.Clone();
                    scheduler.Released = true;
                    _schedulers[key] = scheduler;
                }
            }
        }

        private void CleanSchedulers(string tenant, IEnumerable<Type> taskTypes)
        {
            var validKeys = taskTypes.Select(type => tenant + type.FullName).ToArray();

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
        public static HttpContext GetHttpContext(this ShellContext shell)
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
        public static IEnumerable<Type> GetTaskTypes(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().Select(t => t.GetType());
        }

        public static IBackgroundTask GetTaskByTypeName(this IServiceScope scope, string type)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().FirstOrDefault(t => t.GetType().FullName == type);
        }

        public static async Task<BackgroundTaskSettings> GetTaskSettingsAsync(this IServiceScope scope, Type type)
        {
            var providers = scope.ServiceProvider.GetService<IOptions<BackgroundTaskOptions>>().Value.SettingsProviders;

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

    internal static class LoggerExtensions
    {
        public static void Information(this ILogger logger, string message)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(message);
            }
        }

        public static void Error(this ILogger logger, Exception e, string message)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(e, message);
            }
        }
    }
}