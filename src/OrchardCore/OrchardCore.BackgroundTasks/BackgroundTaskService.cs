using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskService : IBackgroundTaskService, IDisposable
    {
        private static TimeSpan DontStart = TimeSpan.FromMilliseconds(-1);
        private static TimeSpan Delay = TimeSpan.FromSeconds(1);

        private readonly Dictionary<string, IEnumerable<IBackgroundTask>> _tasks;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _orchardHost;
        private readonly ShellSettings _shellSettings;
        private readonly Dictionary<IBackgroundTask, BackgroundTaskState> _states;
        private readonly Dictionary<string, Timer> _timers;
        private readonly Dictionary<string, TimeSpan> _periods;

        public BackgroundTaskService(
            IShellHost orchardHost,
            ShellSettings shellSettings,
            IApplicationLifetime applicationLifetime,
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IBackgroundTask> tasks,
            ILogger<BackgroundTaskService> logger)
        {
            _shellSettings = shellSettings;
            _orchardHost = orchardHost;
            _applicationLifetime = applicationLifetime;
            _httpContextAccessor = httpContextAccessor;
            _tasks = tasks.GroupBy(GetGroupName).ToDictionary(x => x.Key, x => x.Select(i => i));
            _states = tasks.ToDictionary(x => x, x => BackgroundTaskState.Idle);
            _timers = _tasks.Keys.ToDictionary(x => x, x => CreateTimer(DoWorkAsync, x));
            _periods = _tasks.Keys.ToDictionary(x => x, x => TimeSpan.FromMinutes(1));
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public void Activate()
        {
            if (_shellSettings.State == TenantState.Running)
            {
                foreach (var group in _timers.Keys)
                {
                    var timer = _timers[group];
                    var period = _periods[group];
                    timer.Change(Delay, period);
                }
            }
        }

        // NB: Async void should be avoided; it should only be used for event handlers.Timer.Elapsed is an event handler.So, it's not necessarily wrong here.
        // c.f. http://stackoverflow.com/questions/25007670/using-async-await-inside-the-timer-elapsed-event-handler-within-a-windows-servic
        private async void DoWorkAsync(object group)
        {
            // DoWork needs to be re-entrant as Timer may call the callback before the previous callback has returned.
            // So, because a task may take longer than the period itself, DoWork needs to check if it's still running.

            var groupName = group as string ?? "";

            // Because the execution flow has been suppressed before creating timers, here the 'HttpContext'
            // is always null and can be replaced by a new fake 'HttpContext' without overriding anything.
            _httpContextAccessor.HttpContext = new DefaultHttpContext();

            foreach (var task in _tasks[groupName])
            {
                var taskName = task.GetType().FullName;

                using (var scope = await _orchardHost.GetScopeAsync(_shellSettings))
                {
                    try
                    {
                        if (_states[task] != BackgroundTaskState.Idle)
                        {
                            return;
                        }

                        lock (_states)
                        {
                            // Ensure Terminate() was not called before
                            if (_states[task] != BackgroundTaskState.Idle)
                            {
                                return;
                            }

                            _states[task] = BackgroundTaskState.Running;
                        }

                        if (Logger.IsEnabled(LogLevel.Information))
                        {
                            Logger.LogInformation("Start processing background task '{BackgroundTaskName}'.", taskName);
                        }

                        await task.DoWorkAsync(scope.ServiceProvider, _applicationLifetime.ApplicationStopping);

                        if (Logger.IsEnabled(LogLevel.Information))
                        {
                            Logger.LogInformation("Finished processing background task '{BackgroundTaskName}'.", taskName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error while processing background task '{BackgroundTaskName}'", taskName);
                    }
                    finally
                    {
                        lock (_states)
                        {
                            // Ensure Terminate() was not called during the task
                            if (_states[task] != BackgroundTaskState.Stopped)
                            {
                                _states[task] = BackgroundTaskState.Idle;
                            }
                        }
                    }
                }
            }
        }

        public void Terminate()
        {
            lock (_states)
            {
                var tasks = _states.Keys.ToArray();

                foreach (var task in tasks)
                {
                    _states[task] = BackgroundTaskState.Stopped;
                }
            }
        }

        private string GetGroupName(IBackgroundTask task)
        {
            var attributes = task.GetType().GetCustomAttributes<BackgroundTaskAttribute>().ToList();

            if (attributes.Count == 0)
            {
                return "";
            }

            return attributes.First().Group ?? "";
        }

        public IDictionary<IBackgroundTask, BackgroundTaskState> GetTasks()
        {
            return _states;
        }

        public void SetDelay(string group, TimeSpan period)
        {
            var groupName = group ?? "";

            _periods[groupName] = period;
            _timers[groupName].Change(DontStart, period);
        }

        public void Dispose()
        {
            foreach(var timer in _timers.Values)
            {
                timer.Dispose();
            }
        }

        [SecuritySafeCritical]
        private static Timer CreateTimer(TimerCallback callback, string name)
        {
            // Prevent the current execution context from being captured to ensure async-local
            // elements like the HTTP context of the initiating request are not flowed and accessible
            // from background tasks, which would prevent the HTTP context instance from being GCed.

            var restore = false;
            try
            {
                if (!ExecutionContext.IsFlowSuppressed())
                {
                    ExecutionContext.SuppressFlow();
                    restore = true;
                }

                return new Timer(callback, name, Timeout.Infinite, Timeout.Infinite);
            }
            finally
            {
                if (restore)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }
    }
}
