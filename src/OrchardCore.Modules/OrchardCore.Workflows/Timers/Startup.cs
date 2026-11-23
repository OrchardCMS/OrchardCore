using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Jobs;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Workflows.Timers;

[Feature("OrchardCore.Workflows.Timers")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<TimerEvent, TimerEventDisplayDriver>(activity => activity.Icon = "fa-solid fa-clock");

        // The timers are evaluated every minute by one server. Its runs are not kept, as there is one per minute.
        services.AddJob<WorkflowTimerJob>(WorkflowTimerJob.Name, job =>
        {
            job.Title = WorkflowsLocalizationSources.TimerJobTitle;
            job.Description = WorkflowsLocalizationSources.TimerJobDescription;
            job.Schedule = "* * * * *";
            job.KeepSucceededRuns = false;
        });
    }
}
