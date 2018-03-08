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

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundHostedService : BackgroundService
    {
        private static TimeSpan PeriodTime = TimeSpan.FromMinutes(1);
        private static TimeSpan MinIdleTime = TimeSpan.FromSeconds(10);
        private Dictionary<string, Scheduler> _schedulers = new Dictionary<string, Scheduler>();

        private readonly IShellHost _shellHost;

        public BackgroundHostedService(
            IShellHost shellHost,
            ILogger<BackgroundHostedService> logger)
        {
            _shellHost = shellHost;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => Logger.LogDebug($"BackgroundHostedService is stopping."));

            var startedUtc = DateTime.UtcNow;

            while (!cancellationToken.IsCancellationRequested)
            {
                var periodDelay = Task.Delay(PeriodTime, cancellationToken);

                CleanSchedulers();

                var shellContexts = GetRunningShellContexts();

                foreach (var shellContext in shellContexts)
                {
                    if (shellContext.Released)
                    {
                        continue;
                    }

                    IEnumerable<Type> taskTypes;

                    using (var scope = shellContext.EnterServiceScope())
                    {
                        taskTypes = scope.GetBackgroundTaskTypes();
                    }

                    var tenant = shellContext.Settings.Name;

                    foreach (var taskType in taskTypes)
                    {
                        var taskName = taskType.FullName;

                        if (shellContext.Released)
                        {
                            break;
                        }

                        using (var scope = shellContext.EnterServiceScope())
                        {
                            var task = scope.GetBackgroundTaskOfType(taskType);

                            if (task == null)
                            {
                                continue;
                            }

                            if (!_schedulers.TryGetValue(tenant + taskName, out Scheduler scheduler))
                            {
                                _schedulers[tenant + taskName] = scheduler =
                                    new Scheduler(shellContext, task, startedUtc);
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

                startedUtc = DateTime.UtcNow;
                await Task.Delay(MinIdleTime, cancellationToken);
                await periodDelay;
            }
        }

        private IEnumerable<ShellContext> GetRunningShellContexts()
        {
            return _shellHost.ListShellContexts()?.Where(s => s.Settings.State == TenantState.Running)
                .OrderBy(s => s.Settings.Name).ToArray() ?? Enumerable.Empty<ShellContext>();
        }

        private void CleanSchedulers()
        {
            var schedulers = _schedulers.Where(kv => kv.Value.ShellContext.Released).Select(kv => kv.Key).ToArray();

            foreach (var scheduler in schedulers)
            {
                _schedulers.Remove(scheduler);
            }
        }

        private class Scheduler
        {
            public Scheduler(ShellContext shellContext, IBackgroundTask task, DateTime startUtc)
            {
                var attribute = task.GetType().GetCustomAttribute<BackgroundTaskAttribute>();
                Schedule = attribute?.Schedule ?? "* * * * *";
                ShellContext = shellContext;
                StartUtc = startUtc;
            }

            public string Schedule { get; }
            public ShellContext ShellContext { get; }
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

    internal static class ServiceScopeExtensions
    {
        public static IEnumerable<Type> GetBackgroundTaskTypes(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().Select(t => t.GetType());
        }

        public static IBackgroundTask GetBackgroundTaskOfType(this IServiceScope scope, Type type)
        {
            return scope.ServiceProvider.GetServices<IBackgroundTask>().FirstOrDefault(t => t.GetType() == type);
        }
    }
}