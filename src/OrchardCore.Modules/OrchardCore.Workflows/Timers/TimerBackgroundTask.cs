using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Timers
{
    [BackgroundTask(Schedule = "* * * * *", Description = "Trigger workflow timer events.")]
    public class TimerBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var workflowManager = serviceProvider.GetRequiredService<IWorkflowManager>();
            return workflowManager.TriggerEventAsync(TimerEvent.EventName, null, null, isExclusive: true, isAlwaysCorrelated: true);
        }
    }
}
