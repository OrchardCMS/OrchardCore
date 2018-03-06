using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundTasks
{
    public class HostedBackgroundService : BackgroundService
    {
        private Dictionary<string, Scheduler> _schedulers = new Dictionary<string, Scheduler>();

        private readonly IShellHost _shellHost;

        public HostedBackgroundService(
            IShellHost shellHost,
            ILogger<HostedBackgroundService> logger)
        {
            _shellHost = shellHost;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => Logger.LogDebug($"HostedBackgroundService is stopping."));

            var referenceTime = DateTime.UtcNow;

            while (!cancellationToken.IsCancellationRequested)
            {
                var shellContexts = _shellHost.ListShellContexts() ?? Enumerable.Empty<ShellContext>();

                var tenants = shellContexts.Select(s => s.Settings.Name);

                var schedulersToRemove = _schedulers
                    .Where(kv => !tenants.Contains(kv.Value.Tenant))
                    .Select(kv => kv.Key);

                foreach (var schedulerToRemove in schedulersToRemove)
                {
                    _schedulers.Remove(schedulerToRemove);
                }

                foreach (var shellContext in shellContexts)
                {
                    if (shellContext.Released || shellContext.Settings.State != TenantState.Running)
                    {
                        continue;
                    }

                    IEnumerable<Type> taskTypes;

                    using (var scope = shellContext.EnterServiceScope())
                    {
                        taskTypes = scope.ServiceProvider.GetServices<IBackgroundTask>()
                            .Select(t => t.GetType());
                    }

                    var tenant = shellContext.Settings.Name;

                    foreach (var taskType in taskTypes)
                    {
                        var taskName = taskType.FullName;

                        using (var scope = shellContext.EnterServiceScope())
                        {
                            var task = scope.ServiceProvider.GetServices<IBackgroundTask>()
                                .FirstOrDefault(t => t.GetType() == taskType);

                            if (task == null)
                            {
                                continue;
                            }

                            if (!_schedulers.TryGetValue(tenant + taskName, out Scheduler scheduler))
                            {
                                scheduler = new Scheduler(tenant, task, referenceTime);
                                _schedulers[tenant + taskName] = scheduler;
                            }

                            if (!scheduler.ShouldRun())
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

                                if (!shellContext.IsActivated)
                                {
                                    lock (shellContext)
                                    {
                                        if (!shellContext.IsActivated)
                                        {
                                            var tenantEvents = scope.ServiceProvider
                                                .GetServices<IModularTenantEvents>();

                                            foreach (var tenantEvent in tenantEvents)
                                            {
                                                tenantEvent.ActivatingAsync().Wait();
                                            }

                                            shellContext.IsActivated = true;

                                            foreach (var tenantEvent in tenantEvents)
                                            {
                                                tenantEvent.ActivatedAsync().Wait();
                                            }
                                        }
                                    }
                                }

                                shellContext.RequestStarted();

                                await task.DoWorkAsync(scope.ServiceProvider, cancellationToken);

                                if (Logger.IsEnabled(LogLevel.Information))
                                {
                                    Logger.LogInformation(
                                        "Finished processing background task \"{0}\" on tenant \"{1}\".",
                                        tenant, taskName);
                                }
                            }

                            catch (Exception ex)
                            {
                                if (Logger.IsEnabled(LogLevel.Error))
                                {
                                    Logger.LogError(ex,
                                        "Error while processing background task \"{0}\" on tenant \"{1}\".",
                                        tenant, taskName);
                                }
                            }

                            finally
                            {
                                shellContext.RequestEnded();

                                if (shellContext.CanTerminate)
                                {
                                    var tenantEvents = scope.ServiceProvider
                                        .GetServices<IModularTenantEvents>();

                                    foreach (var tenantEvent in tenantEvents)
                                    {
                                        await tenantEvent.TerminatingAsync();
                                    }

                                    foreach (var tenantEvent in tenantEvents.Reverse())
                                    {
                                        await tenantEvent.TerminatedAsync();
                                    }

                                    shellContext.Dispose();
                                }
                            }
                        }

                        if (shellContext.Released || shellContext.Settings.State != TenantState.Running)
                        {
                            break;
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        private class Scheduler
        {
            public Scheduler(string tenant, IBackgroundTask task, DateTime startUtc)
            {
                Tenant = tenant;
                var attribute = task.GetType().GetCustomAttribute<BackgroundTaskAttribute>();
                Schedule = attribute?.Schedule ?? "* * * * *";
                StartUtc = startUtc;
            }

            public string Tenant { get; }
            public string Schedule { get; }
            public DateTime StartUtc { get; set; }

            public bool ShouldRun()
            {
                if (DateTime.UtcNow > CrontabSchedule.Parse(Schedule).GetNextOccurrence(StartUtc))
                {
                    StartUtc = DateTime.UtcNow;
                    return true;
                }

                return false;
            }
        }
    }
}